
using Inspector.Framework.Helpers.Extensions;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Models;
using Inspector.Resources.Labels;
using NativeMedia;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class AddReportPageViewModel : BaseViewModel
    {
        TicketClient _ticketClient;
        User _userAccount;
        ValidationUnit _validationUnit;
        Ticket _editingTicket;

        public AddReportPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            StateSelected = Validator.Build<int>()
                            .Must(x => x > 0, Message.FieldRequired);

            ID = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule());

            PhoneNumber = Validator.Build<string>()
                            .When(x => !string.IsNullOrEmpty(x))
                            .Must(x => x.Length >= 10, Message.MaxMinInvalidField);

            Title = Validator.Build<string>()
                            .IsRequired(Message.FieldRequired)
                            .Must(x => x.Length >= 10, Message.MaxMinInvalidField);

            Address = Validator.Build<string>()
                            .IsRequired(Message.FieldRequired)
                            .Must(x => x.Length >= 10, Message.MaxMinInvalidField);

            GroupSelected = Validator.Build<int>()
                            .Must(x => x > 0, Message.FieldRequired);

            Comments = Validator.Build<string>()
                            .When(x => !string.IsNullOrEmpty(x))
                            .Must(x => x.Length >= 10, Message.MaxMinInvalidField);

            _validationUnit = new ValidationUnit(StateSelected, ID, PhoneNumber, Title, Address, GroupSelected, Comments);

            States = new List<StateTicket>(StateTicket.GetStatesForNewTicket());

            ReportCommand = new DelegateCommand(ReportCommandExecute);
            SelectStateCommand = new DelegateCommand<StateTicket>(state => StateSelected.Value = state.State);
            SelectGroupCommand = new DelegateCommand<Group>(group => GroupSelected.Value = group.Id);
            AttachFileCommand = new DelegateCommand(OnAttachFileCommandExecute);
            ShowFilesCommand = new DelegateCommand(OnShowFilesCommandExecute);

            Init();
        }

        public string PageTitle { get; set; } = "Crear Reporte";

        bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                PageTitle = value ? "Editar reporte" : "Crear Reporte";
            }
        }

        public List<StateTicket> States { get; set; }
        public Validatable<int> StateSelected { get; set; }
        public Validatable<string> ID { get; set; }
        public string CitizenName { get; set; }
        public Validatable<string> PhoneNumber { get; set; }
        public Validatable<string> Title { get; set; }
        public Validatable<string> Address { get; set; }
        public int CategorySelected { get; set; }
        public List<Group> Groups { get; set; }
        public Validatable<int> GroupSelected { get; set; }
        public Validatable<string> Comments { get; set; }
        //public int IncidentSelected { get; set; }
        //public int xSelected { get; set; }
        //public int InstitutionSelected { get; set; }
        public ObservableCollection<IMediaFile> Attachements { get; set; } = new ObservableCollection<IMediaFile>();

        public ICommand ReportCommand { get; set; }
        public ICommand SelectStateCommand { get; set; }
        public ICommand SelectGroupCommand { get; set; }
        public ICommand AttachFileCommand { get; set; }
        public ICommand ShowFilesCommand { get; set; }

        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();

            _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);
            Groups = await _cacheService.GetLocalObject<List<Group>>(CacheKeys.Groups);
        }

        private async void ReportCommandExecute()
        {
            if (IsBusy)
                return;

            if (!_validationUnit.Validate())
            {
                await _dialogService.DisplayAlertAsync("Ups :(", "Verifique que todas las propiedades estan correctas.", "Ok");
                return;
            }

            IsBusy = true;

            try
            {
                List<TicketAttachment> attachements = null;

                if (Attachements.Count > 0)
                {
                    attachements = new List<TicketAttachment>();
                    foreach (var file in Attachements)
                    {
                        var fileName = string.IsNullOrEmpty(file.NameWithoutExtension) ? "evidencia" : file.NameWithoutExtension;

                        attachements.Add(new TicketAttachment
                        {
                            Filename = fileName + "." + file.Extension,
                            MimeType = file.ContentType,
                            Data = (await file.OpenReadAsync()).ConvertToStringBase64() 
                        });

                        file.Dispose();
                    }          
                }

                var formTicket = new Ticket
                {
                    Title = Title.Value,
                    GroupId = StateSelected.Value,
                    CustomerId = _userAccount.Id,
                    OwnerId = _userAccount.Id,
                    StateId = GroupSelected.Value,
                    CustomAttributes = new Dictionary<string, object>()
                    {
                        { "address",  Address.Value },
                    }
                };                

                Ticket ticket;

                if(IsEditing)
                    ticket = await _ticketClient.UpdateTicketAsync(_editingTicket.Id, formTicket);
                else
                {
                    var formTicketArticle = new TicketArticle
                    {
                        Subject = Title.Value,
                        Body = Comments.Value,
                        Type = "note",
                        Attachments = attachements
                    };

                    ticket = await _ticketClient.CreateTicketAsync(formTicket, formTicketArticle);
                }                    

                if (ticket.Id <= 0)                
                    await _dialogService.DisplayAlertAsync("", Message.TicketNotCreated, "Ok");
                else
                {
                    await _dialogService.DisplayAlertAsync(":)", Message.TicketCreated, "Ok");

                    var parameters = new NavigationParameters()
                    {
                        { NavigationKeys.NewTicket, ticket }
                    };
                    await _navigationService.GoBackAsync(parameters);
                }                         
            }
            catch (Zammad.Client.Core.ZammadException e)
            {
#if DEBUG || DEBUG_AGENT
                var content = await e.Response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
#endif
                await _dialogService.DisplayAlertAsync("Ups :(", Message.SomethingHappen, "Ok");
            }

            IsBusy = false;
        }

        private async void OnAttachFileCommandExecute()
        {
            //IActionSheetButton imageAction = ActionSheetButton.CreateButton("Image", () => { Debug.WriteLine("Select A"); });
            //var response = await _dialogService.DisplayActionSheetAsync("Tipo de archivo", "Cancelar", null, "Imagen", "Video");

            var cts = new CancellationTokenSource();

            try
            {
                var request = new MediaPickRequest(5, MediaFileType.Image, MediaFileType.Video)
                {
                    PresentationSourceBounds = System.Drawing.Rectangle.Empty,
                    Title = "Select"
                };

                cts.CancelAfter(TimeSpan.FromMinutes(3));

                var results = await MediaGallery.PickAsync(request, cts.Token);

                if(results?.Files != null)
                    Attachements = new ObservableCollection<IMediaFile>(results?.Files);
            }
            //catch (OperationCanceledException)
            //{
            //    // handling a cancellation request
            //}
            catch (Exception)
            {
                await _dialogService.DisplayAlertAsync("Ups :(", Message.SomethingHappen, "Ok");
            }
            finally
            {
                cts.Dispose();
            }
        }

        private async void OnShowFilesCommandExecute()
        {
            var parameters = new NavigationParameters()
            {
                { NavigationKeys.ShowFiles, Attachements }
            };
            await _navigationService.NavigateAsync(NavigationKeys.GalleryPage, parameters);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(NavigationKeys.RemoveAllFiles))
            {
                Attachements = new ObservableCollection<IMediaFile>();
            }

            if (parameters.ContainsKey(NavigationKeys.IsEditing))
            {
                IsEditing = true;
                _editingTicket = parameters.GetValue<Ticket>(NavigationKeys.IsEditing);
            }
        }
    }
}

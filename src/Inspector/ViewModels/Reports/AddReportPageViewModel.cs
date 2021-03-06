
using Inspector.Framework.Dtos;
using Inspector.Framework.Helpers;
using Inspector.Framework.Helpers.Extensions;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using Microsoft.AppCenter.Crashes;
using NativeMedia;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Plugin.ValidationRules.Formatters;
using Prism.Commands;
using Prism.Logging;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
using XF.Material.Forms.UI.Dialogs;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class AddReportPageViewModel : IncidentsViewModel
    {
        TicketClient _ticketClient;
        UserClient _userClient;
        TagClient _tagClient;
        User _userAccount;
        User _clientAccount;
        ValidationUnit _validationUnit;
        Ticket _editingTicket;
        ICitizenAPI _citizenClient;
        Citizen _citizen;
        bool _documentValidated, _clientCreated;
        ILogger _logger;

        public AddReportPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ILogger logger,
            ICacheService cacheService, ITerritorialDivisionAPI territorialDivisionClient, ICitizenAPI citizenClient, IIncidentsAPI incidentsClient)
            : base(navigationService, dialogService, logger, cacheService, incidentsClient, territorialDivisionClient)
        {
            _citizenClient = citizenClient;
            _logger = logger;
            //StateSelected = Validator.Build<int>()
            //                .Must(x => x > 0, Message.FieldRequired);

            ID = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule())
                .Must(x => x?.Replace("-", "") != _userAccount?.CustomAttributes["cedula"]?.ToString()?.Replace("-", ""), Message.SameId);

            ID.ValueFormatter = new MaskFormatter("XXX-XXXXXXX-X");

            PhoneNumber = Validator.Build<string>()
                            .When(x => !string.IsNullOrEmpty(x))
                            .Must(x => x?.Length == 12, Message.MinLengthField12);

            PhoneNumber.ValueFormatter = new MaskFormatter("XXX-XXX-XXXX");

            //Title = Validator.Build<string>()
            //                .IsRequired(Message.FieldRequired)
            //                .Must(x => x?.Length >= 6, Message.MinLengthField6);

            Address = Validator.Build<string>()
                            .IsRequired(Message.FieldRequired)
                            .Must(x => x?.Length >= 6, Message.MinLengthField6);

            GroupSelected = Validator.Build<int>()
                            .Must(x => x > 0, Message.FieldRequired);

            Comments = Validator.Build<string>()
                            .Must(x => x?.Length >= 10, Message.MinLengthField10);

            _validationUnit = new ValidationUnit(/*StateSelected,*/ ID, PhoneNumber, /*Title,*/ Address, GroupSelected, Neighhborhood, SubCategory, Comments);

            //States = new List<StateTicket>(StateTicket.GetStatesForNewTicket());

            ReportCommand = new DelegateCommand(async () => 
            {
                using (await MaterialDialog.Instance.LoadingDialogAsync(message: "Por favor, espere..."))
                {
                    await ReportCommandExecute();
                }                
            });
            //SelectStateCommand = new DelegateCommand<StateTicket>(state => StateSelected.Value = state.State);
            SelectGroupCommand = new DelegateCommand<Group>(group => GroupSelected.Value = group.Id);
            AttachFileCommand = new DelegateCommand(OnAttachFileCommandExecute);
            ShowFilesCommand = new DelegateCommand(OnShowFilesCommandExecute);
            ValidateIDCommand = new DelegateCommand(OnValidateIDCommandExecute);

            Init();
        }

        #region Properties
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

        //public TerritorialDivision TerritorialDivisions { get; set; }
        //public List<StateTicket> States { get; set; }
        //public Validatable<int> StateSelected { get; set; }
        public Validatable<string> ID { get; set; }
        public string CitizenName { get; set; } = "Nombre del ciudadano";
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
        public bool IsValidatingDocument { get; set; }
        #endregion

        #region Commands
        public ICommand ReportCommand { get; set; }
        public ICommand SelectStateCommand { get; set; }
        public ICommand SelectGroupCommand { get; set; }
        public ICommand AttachFileCommand { get; set; }
        public ICommand ShowFilesCommand { get; set; }
        public ICommand ValidateIDCommand { get; set; }
        #endregion

        private async void Init()
        {
            try
            {
                var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
                _ticketClient = account.CreateTicketClient();
                _userClient = account.CreateUserClient();
                _tagClient = account.CreateTagClient();
                var groupClient = account.CreateGroupClient();

                _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);

                Groups = (await groupClient.GetGroupListAsync()).Where(x => x.Active).OrderBy(x => x.Name).ToList();
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                _logger.Report(e);
                await _dialogService.DisplayAlertAsync("Ups :(", Message.GroupNotLoaded, "Ok");
                await _navigationService.GoBackAsync();
            }
        }

        private async void OnValidateIDCommandExecute()
        {
            try
            {
                if (IsValidatingDocument || !ID.Validate())
                    return;

                IsValidatingDocument = true;

                var id = ID.Value.Replace("-", "");
                var users = await _userClient.SearchUserAsync(id, 1);

                if (users?.Count > 0)
                {
                    _clientAccount = users[0];
                    CitizenName = _clientAccount.FirstName + " " + _clientAccount.LastName;
                    PhoneNumber.Value = _clientAccount.Phone;
                    _clientCreated = true;
                    return;
                }
                
                var resp = await _citizenClient.GetCitizenBasicInfo(id);

                if (resp != null && resp.Valid)
                {
                    _citizen = resp.Payload;
                    CitizenName = _citizen.Names + " " + _citizen.FirstSurname + " " + _citizen.SecondSurname;
                    _clientCreated = false;
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                _logger.Report(e);
            }
            finally
            {
                IsValidatingDocument = false;
                _documentValidated = true;
            }
        }

        private async Task ReportCommandExecute()
        {
            if (IsBusy)
                return;

            if (!_validationUnit.Validate() || !_documentValidated)
            {
                await _dialogService.DisplayAlertAsync("Ups :(", Message.AllFieldsInvalid, "Ok");
                return;
            }

            IsBusy = true;

            Ticket ticket = null;
            var ticketCreated = false;

            try
            {
                var customerId = _userAccount.Id;
                string zoneCode = GetZone()?.Id;
                string phoneNumber = PhoneNumber?.Value?.Replace("-", "");

                if (!_clientCreated)
                {
                    var newUser = await _userClient.CreateUserAsync(new User
                    {
                        FirstName = _citizen.Names,
                        LastName = _citizen.FirstSurname + " " + _citizen.SecondSurname,
                        Phone = phoneNumber,
                        CustomAttributes = new Dictionary<string, object>()
                        {
                            { "cedula",  ID.Value.Replace("-", "") },
                            { "zone",  zoneCode },
                        }
                    });

                    if (newUser == null)
                        return;

                    _clientAccount = newUser;
                    _clientCreated = true;
                }
                else if(PhoneNumber.IsValid && _clientAccount.Phone != phoneNumber)
                {
                    await _userClient.UpdateUserAsync(_clientAccount.Id, new User { Phone = phoneNumber });
                    _clientAccount.Phone = phoneNumber;
                }

                customerId = _clientAccount.Id;

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

                var title = SubCategory.Value.Name + " en " + GetZone()?.Name;
                var formTicket = new Ticket
                {
                    Title = title, //Title.Value,
                    GroupId = GroupSelected.Value,
                    CustomerId = customerId,
                    OwnerId = _userAccount.Id,
                    StateId = (int)Framework.Dtos.TicketState.New,
                    
                    CustomAttributes = new Dictionary<string, object>()
                    {
                        { "address",  Address.Value },
                        { "zone",  zoneCode },
                    }
                };                

                if(IsEditing)
                    ticket = await _ticketClient.UpdateTicketAsync(_editingTicket.Id, formTicket);
                else
                {
                    var formTicketArticle = new TicketArticle
                    {
                        Subject = title, //Title.Value,
                        Body = Comments.Value,
                        Type = "note",
                        Attachments = attachements
                    };

                    ticket = await _ticketClient.CreateTicketAsync(formTicket, formTicketArticle);
                }                    

                if (ticket == null || ticket?.Id <= 0)                
                    await _dialogService.DisplayAlertAsync("", Message.TicketNotCreated, "Ok");
                else if(ticket.OwnerId != _userAccount.Id)
                    await _dialogService.DisplayAlertAsync("", "El ticket se ha creado pero es posible que no tengas los permisos suficientes para verlo.", "Ok");
                else
                {
                    ticketCreated = true;
                    await AddTagsToTicket(ticket.Id);                   
                }                         
            }
            catch (Zammad.Client.Core.ZammadException e)
            {
                var content = await e.Response.Content.ReadAsStringAsync();
                _logger.Report(e, LoggerExtension.InitDictionary(content));
            }
            catch (Exception e)
            {
                _logger.Report(e);
            }
            finally
            {
                if (ticketCreated)
                {
                    await _dialogService.DisplayAlertAsync(":)", Message.TicketCreated, "Ok");

                    var parameters = new NavigationParameters()
                    {
                        { NavigationKeys.NewTicket, ticket }
                    };
                    await _navigationService.GoBackAsync(parameters);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("", Message.SomethingHappen, "Ok");
                }

                IsBusy = false;
            }

        }

        private async Task AddTagsToTicket(int id)
        {
            try
            {
                if (Incident.Validate())
                    await _tagClient.AddTagAsync("Ticket", id, Incident.Value.Name);

                if (Category.Validate())
                    await _tagClient.AddTagAsync("Ticket", id, Category.Value.Name);

                if (SubCategory.Validate())
                    await _tagClient.AddTagAsync("Ticket", id, SubCategory.Value.Name);
            }
            catch (Exception e)
            {
                _logger.Report(e);
            }
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
                    Title = "Selecciona"
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
            catch (Exception e)
            {
                _logger.Report(e);
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

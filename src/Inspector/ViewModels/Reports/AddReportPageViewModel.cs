
using Inspector.Framework.Dtos;
using Inspector.Framework.Helpers.Extensions;
using Inspector.Framework.Interfaces;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Resources.Labels;
using NativeMedia;
using Plugin.ValidationRules;
using Plugin.ValidationRules.Extensions;
using Plugin.ValidationRules.Formatters;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UIModule.Helpers.Rules;
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

        public AddReportPageViewModel(INavigationService navigationService, IPageDialogService dialogService, 
            ICacheService cacheService, ITerritorialDivisionAPI territorialDivisionClient, ICitizenAPI citizenClient, IIncidentsAPI incidentsClient) 
            : base(navigationService, dialogService, cacheService, incidentsClient, territorialDivisionClient)
        {
            _citizenClient = citizenClient;

            //StateSelected = Validator.Build<int>()
            //                .Must(x => x > 0, Message.FieldRequired);

            ID = Validator.Build<string>()
                .IsRequired(Message.FieldRequired)
                .WithRule(new CedulaRule())
                .Must(x => x?.Replace("-", "") != _userAccount?.CustomAttributes["cedula"]?.ToString()?.Replace("-", ""), Message.SameId);

            ID.ValueFormatter = new MaskFormatter("XXX-XXXXXXX-X");

            PhoneNumber = Validator.Build<string>()
                            .When(x => !string.IsNullOrEmpty(x))
                            .Must(x => x.Length == 12, Message.MinLengthField12);

            PhoneNumber.ValueFormatter = new MaskFormatter("XXX-XXX-XXXX");

            Title = Validator.Build<string>()
                            .IsRequired(Message.FieldRequired)
                            .Must(x => x?.Length >= 6, Message.MinLengthField6);

            Address = Validator.Build<string>()
                            .IsRequired(Message.FieldRequired)
                            .Must(x => x.Length >= 6, Message.MinLengthField6);

            GroupSelected = Validator.Build<int>()
                            .Must(x => x > 0, Message.FieldRequired);

            Comments = Validator.Build<string>()
                            .When(x => !string.IsNullOrEmpty(x))
                            .Must(x => x.Length >= 10, Message.MinLengthField10);

            _validationUnit = new ValidationUnit(/*StateSelected,*/ ID, /*PhoneNumber,*/ Title, Address, GroupSelected, Neighhborhood, SubCategory, Comments);

            //States = new List<StateTicket>(StateTicket.GetStatesForNewTicket());

            ReportCommand = new DelegateCommand(ReportCommandExecute);
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
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _ticketClient = account.CreateTicketClient();
            _userClient = account.CreateUserClient();
            _tagClient = account.CreateTagClient();
            var groupClient = account.CreateGroupClient();

            _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);

            Groups = (List<Group>)await groupClient.GetGroupListAsync();
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
                
            }
            finally
            {
                IsValidatingDocument = false;
                _documentValidated = true;
            }
        }

        private async void ReportCommandExecute()
        {
            if (IsBusy)
                return;

            if (!_validationUnit.Validate() || !_documentValidated)
            {
                await _dialogService.DisplayAlertAsync("Ups :(", "Verifique que todas las propiedades estan correctas.", "Ok");
                return;
            }

            PhoneNumber.Validate(); // when condition is returning false even if there is not errors. Validations occurs here for now
            IsBusy = true;
            var customerId = _userAccount.Id;

            Ticket ticket = null;
            var ticketCreated = false;
            string zoneCode = GetZoneCode();

            try
            {
                if (!_clientCreated)
                {
                    var newUser = await _userClient.CreateUserAsync(new User
                    {
                        FirstName = _citizen.Names,
                        LastName = _citizen.FirstSurname + " " + _citizen.SecondSurname,
                        Phone = PhoneNumber.Value.Replace("-", ""),
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

                var formTicket = new Ticket
                {
                    Title = Title.Value,
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
                        Subject = Title.Value,
                        Body = Comments.Value,
                        Type = "note",
                        Attachments = attachements
                    };

                    ticket = await _ticketClient.CreateTicketAsync(formTicket, formTicketArticle);
                }                    

                if (ticket?.Id <= 0)                
                    await _dialogService.DisplayAlertAsync("", Message.TicketNotCreated, "Ok");
                else
                {
                    ticketCreated = true;
                    await AddTagsToTicket(ticket.Id);                   
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
            catch { }
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

                IsBusy = false;
            }

        }

        private string GetZoneCode()
        {
            if (SubNeighhborhood.IsValid)
                return SubNeighhborhood.Value.Code;

            return Neighhborhood.Value.Code;
        }

        private async Task AddTagsToTicket(int id)
        {
            if(Incident.Validate())
                await _tagClient.AddTagAsync("Ticket", id, Incident.Value.Name);

            if (Category.Validate())
                await _tagClient.AddTagAsync("Ticket", id, Category.Value.Name);

            if (SubCategory.Validate())
                await _tagClient.AddTagAsync("Ticket", id, SubCategory.Value.Name);
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

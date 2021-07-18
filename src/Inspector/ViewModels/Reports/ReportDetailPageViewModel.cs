﻿using Inspector.Framework.Helpers.Extensions;
using Inspector.Framework.Services;
using Inspector.Framework.Utils;
using Inspector.Models;
using Inspector.Resources.Labels;
using NativeMedia;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using XF.Material.Forms.UI;
using XF.Material.Forms.UI.Dialogs;
using Zammad.Client;
using Zammad.Client.Resources;

namespace Inspector.ViewModels
{
    public class ReportDetailPageViewModel : BaseViewModel
    {
        TicketClient _ticketClient;
        User _userAccount;

        public ReportDetailPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ICacheService cacheService) : base(navigationService, dialogService, cacheService)
        {
            ShowFilesCommand = new DelegateCommand<Comment>(OnShowFilesCommandExecute);
            SendCommentCommand = new DelegateCommand(OnSendCommentCommandExecute);
            AttachFileCommand = new DelegateCommand(OnAttachFileCommandExecute);
            ChangeStatusTicketCommand = new DelegateCommand(OnChangeStatusTicketCommandExecute);
            Init();
        }

        public Ticket TicketSelected { get; set; }
        public string NewComment { get; set; }

        public ObservableCollection<Comment> Comments { get; set; }
        public DelegateCommand<Comment> ShowFilesCommand { get; set; }
        public DelegateCommand SendCommentCommand { get; set; }
        public ICommand AttachFileCommand { get; set; }
        public ICommand ChangeStatusTicketCommand { get; set; }

        private async void Init()
        {
            var account = await _cacheService.GetSecureObject<ZammadAccount>(CacheKeys.ZammadAccount);
            _userAccount = await _cacheService.GetSecureObject<User>(CacheKeys.UserAccount);
            _ticketClient = account.CreateTicketClient();
        }

        private async void InitComments()
        {
            if (IsBusy && TicketSelected.ArticleCount == 0)
                return;

            IsBusy = true;

            try
            {
                var ticketArticles = await _ticketClient.GetTicketArticleListForTicketAsync(TicketSelected.Id);
                if (ticketArticles.Count == 0)
                    return;

                var comments = new List<Comment>();
                foreach (var item in ticketArticles)
                {
                    var owner = item.CreatedById == _userAccount.Id;
                    comments.Add(new Comment
                    {
                        Id = item.Id,
                        TicketId = item.TicketId,
                        UserName = item.From,
                        Body = item.Body,
                        CreatedAt = item.CreatedAt,
                        Attachments = item.Attachments,
                        IsOwner = owner
                    });
                }
                Comments = new ObservableCollection<Comment>(comments);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnShowFilesCommandExecute(Comment comment)
        {
            if (!comment.HasAttachments)
                return;

            var parameters = new NavigationParameters()
            {
                //{ NavigationKeys.ShowFiles, comment.Attachments },
                { NavigationKeys.CommentSelected, comment},
            };
            await _navigationService.NavigateAsync(NavigationKeys.PreviewGalleryPage, parameters);
        }

        private async Task CreateComment(string comment, List<TicketAttachment> attachments = null)
        {
            var resp = await _ticketClient.CreateTicketArticleAsync(new TicketArticle
            {
                TicketId = TicketSelected.Id,
                Subject = TicketSelected.Title,
                Body = comment,
                Attachments = attachments
            });

            if (resp?.Id > 0)
            {
                NewComment = "";
                Comments.Add(new Comment
                {
                    Id = resp.Id,
                    TicketId = resp.TicketId,
                    UserName = resp.From,
                    Body = resp.Body,
                    CreatedAt = resp.CreatedAt,
                    Attachments = resp.Attachments,
                    IsOwner = true
                });
            }
        }

        private async void OnAttachFileCommandExecute()
        {
            //IActionSheetButton imageAction = ActionSheetButton.CreateButton("Image", () => { Debug.WriteLine("Select A"); });
            //var response = await _dialogService.DisplayActionSheetAsync("Tipo de archivo", "Cancelar", null, "Imagen", "Video");
            if (IsBusy)
                return;

            IsBusy = true;

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

                if (results?.Files != null)
                {
                    List<TicketAttachment> attachements = null;

                    if (results.Files.ToList().Count > 0)
                    {
                        attachements = new List<TicketAttachment>();
                        foreach (var file in results.Files)
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

                        await CreateComment("Adjuntos", attachements);
                    }                    
                }                   
            }
            catch (Exception)
            {
                await _dialogService.DisplayAlertAsync("Ups :(", Message.SomethingHappen, "Ok");
            }
            finally
            {
                cts.Dispose();
                IsBusy = false;
            }
        }

        private async void OnSendCommentCommandExecute()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(NewComment))
                return;

            IsBusy = true;

            try
            {
                await CreateComment(NewComment);                    
            }
            catch(Exception e)
            {
                await _dialogService.DisplayAlertAsync("Ups :(", Message.SomethingHappen, "Ok");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnChangeStatusTicketCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var choices = new string[]
                {
                    "Abierto",
                    "En progreso",
                    "Cerrado",
                };

                var view = new MaterialRadioButtonGroup() { Choices = choices };
                bool? wasConfirmed = await MaterialDialog.Instance.ShowCustomContentAsync(view, TicketSelected.Title, "Cambiar estado del ticket");

                if (wasConfirmed == null || !(bool)wasConfirmed)
                    return;

                int status = 0;

                if (view.SelectedIndex == 0)
                    status = (int)Framework.Dtos.TicketState.Open;
                else if (view.SelectedIndex == 1)
                    status = (int)Framework.Dtos.TicketState.InProgress;
                else if (view.SelectedIndex == 2)
                    status = (int)Framework.Dtos.TicketState.Closed;

                var updatedTicket = TicketSelected;
                updatedTicket.StateId = status;

                var ticket = await _ticketClient.UpdateTicketAsync(TicketSelected.Id, updatedTicket);
                if (ticket != null)
                    TicketSelected = ticket;
            }
            catch(Zammad.Client.Core.ZammadException e)
            {

            }
            finally
            {
                IsBusy = false;
            }           
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(NavigationKeys.TicketSelected))
            {
                TicketSelected = parameters.GetValue<Ticket>(NavigationKeys.TicketSelected);
                InitComments();
            }
        }

    }

    

}

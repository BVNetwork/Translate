using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Labs.LanguageManager.Business;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;
using EPiServer.Web.Routing;

namespace Epicode.Translate
{
    [GuiPlugIn(DisplayName = "Translate", 
        Area = PlugInArea.AdminMenu, 
        Url = "~/episerver/epicode.translate/translate.aspx")]
    public partial class Translate : ContentWebFormsBase
    {

        internal Injected<ILanguageBranchRepository> InjectedLanguageBranchRepository { get; set; }
        private ILanguageBranchRepository LanguageBranchRepository => InjectedLanguageBranchRepository.Service;

        internal Injected<IContentLoader> InjectedContentLoader { get; set; }
        private IContentLoader ContentLoader => InjectedContentLoader.Service;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SystemMessageContainer.Heading = "Translate";

            ContentDataSource.DataBind();

            if (SourceLanguageDropDownList.Items.Count < 1)
            {
                foreach (var language in LanguageBranchRepository.ListEnabled())
                {
                    SourceLanguageDropDownList.Items.Add(CreateLanguageListItem(language));                    
                    TargetLanguageDropDownList.Items.Add(CreateLanguageListItem(language));
                }
            }
        }

        private static ListItem CreateLanguageListItem(LanguageBranch language)
        {
            if(language.Name == language.Culture.DisplayName)
                return new ListItem(language.Name, language.Culture.Name);

            return new ListItem(language.Name + " (" + language.Culture.DisplayName + ")", language.Culture.Name);
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            MasterPageFile = UriSupport.ResolveUrlFromUIBySettings("MasterPages/EPiServerUI.Master");
            if (EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole("Administrators") == false
                && EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole("WebAdmins") == false)
            {
                throw new AccessDeniedException();
            }
        }
                
        protected void OnClick_Translate(object sender, EventArgs e)
        {
            var existingCulture = new CultureInfo(SourceLanguageDropDownList.SelectedValue);
            var newCulture = new CultureInfo(TargetLanguageDropDownList.SelectedValue);

            var list = new List<ContentReference> { CurrentContentLink };

            if (TranslateDescendentsCheckBox.Checked)
            {
                list.AddRange(ContentLoader.GetDescendents(CurrentContentLink));
            }
            
            var result = new StringBuilder();

            foreach (var descendent in list)
            {
                string message;

                try
                {
                    message = TranslateContent(descendent, existingCulture, newCulture);
                    
                    var blocks = GetBlocks(descendent, existingCulture, newCulture);
                    foreach (var blockReference in blocks)
                    {
                        message += "<br />" + TranslateContent(blockReference, existingCulture, newCulture) + " (block)";
                    }
                }
                catch (Exception ex)
                {
                    message = $"Could not translate {CurrentContentLink.ID} from {existingCulture.DisplayName} to {newCulture.NativeName}.<br /> {ex.Message}<br /> {ex.StackTrace}<br />";
                }

                result.AppendLine("<br />" + message);
            }

            ResultPlaceHolder.Visible = true;
            ResultLiteral.Text = result.ToString();
        }


        private string TranslateContent(ContentReference contentReference, CultureInfo fromCulture, CultureInfo toCulture)
        {
            var languageBranchManager = ServiceLocator.Current.GetInstance<ILanguageBranchManager>();


            var ok = languageBranchManager.TranslateAndCopyDataFromMasterBranch(contentReference,
                fromCulture.Name,
                fromCulture.TwoLetterISOLanguageName,
                toCulture.Name,
                toCulture.TwoLetterISOLanguageName,
                out _,
                AutoPublishCheckBox.Checked);

            if (ok)
            {
                return $"Content {contentReference.ID} translated from {fromCulture.NativeName} to {toCulture.NativeName}";
            }

            return $"{contentReference.ID} not translated from {fromCulture.NativeName} to {toCulture.NativeName}";
        }


        private IEnumerable<ContentReference> GetBlocks(ContentReference descendent, CultureInfo existingCulture, CultureInfo newCulture)
        {
            var page = ContentLoader.Get<ContentData>(descendent, newCulture);
            
            foreach (var property in page.Property)
            {
                if (property.Value is ContentArea contentArea)
                {
                    foreach (var item in contentArea.Items)
                    {
                        if(ContentLoader.TryGet(item.ContentLink, existingCulture, out BlockData _))
                        {
                            yield return item.ContentLink;
                        }
                    }
                }
            }
        }

        private ContentReference _currentLink;
        public override ContentReference CurrentContentLink
        {
            get
            {
                if (ContentReference.IsNullOrEmpty(_currentLink))
                {
                    _currentLink = ServiceLocator.Current.GetInstance<IContentRouteHelper>().ContentLink;
                    if (ContentReference.IsNullOrEmpty(_currentLink))
                    {
                        ContentReference.TryParse(SelectedContentHiddenField.Value, out _currentLink);
                    }
                    if (ContentReference.IsNullOrEmpty(_currentLink))
                    {
                        _currentLink = ContentReference.StartPage;
                        if (ContentReference.IsNullOrEmpty(_currentLink))
                        {
                            _currentLink = ContentReference.RootPage;
                        }
                    }
                }
                return _currentLink;
            }
            set
            {
                _currentLink = value;
            }
        }
    }
}
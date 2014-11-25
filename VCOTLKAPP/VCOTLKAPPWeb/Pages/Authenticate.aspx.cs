using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VCOTLKAPPWeb.Utlilites;

namespace VCOTLKAPPWeb.Pages
{
    public partial class Authenticate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AADAuthHelper.StoreAuthorizationCodeFromRequest(this.Request);
            CloseCurrentWindow();
        }
        private void CloseCurrentWindow()
        {
            string close =
        @"<script type='text/javascript'>
        window.returnValue = true;
        window.close();
        </script>";
            Response.Write(close);
        }

    }
}
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="VCOTLKAPPWeb.Pages.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <script src="https://appsforoffice.microsoft.com/lib/1.1/hosted/office.js" type="text/javascript"></script>
    <script src="../Scripts/jquery-1.9.1.js" type="text/javascript"></script>
    <link href="../AppRead/Home/Home.css" rel="stylesheet" type="text/css" />
    <link href="../AppRead/App.css" rel="stylesheet" type="text/css" />
    <script src="../AppRead/App.js" type="text/javascript"></script>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">

    <asp:ScriptManager ID="ScriptMgr" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <div>
        <asp:Button runat="server" ID="GetData" OnClick="GetData_Click" Text="Get Data from SAP" UseSubmitBehavior="true"/><br />
        <asp:ListView runat="server" ID="DataListView">
            <ItemTemplate>
                <tr runat="server">
                    <td runat="server">
                        <%-- Data-bound content. --%>
                        <asp:Label ID="DataLabel" runat="server"
                            Text="<%# Container.DataItem.ToString()%>" /><br />
                    </td>
                </tr>
            </ItemTemplate>
        </asp:ListView>
    </div>
    </form>

    <script type="text/javascript">
        $(document).ready(function () {
            Office.initialize = function (reason) {
                var hostType = Office.context.mailbox.diagnostics.hostName;
                PageMethods.GetAuthorizeUrl(hostType,
                    function (value) {
                        if (value) {
                            if (hostType.toLowerCase() == "outlook")
                                window.open(value);
                            else
                                window.location = value;
                        }
                    });
            };
        });
    </script>
</body>
</html>

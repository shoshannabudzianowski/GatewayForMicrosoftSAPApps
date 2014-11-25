<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SAP2SharePointWeb.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    </div>
        <div>
          <h3>Data from SAP via Gateway to Microsoft</h3>
          <asp:ListView runat="server" ID="DataListView">
            <ItemTemplate>
              <tr runat="server">
                <td runat="server">
                  <asp:Label ID="DataLabel" runat="server"
                    Text="<%# Container.DataItem.ToString()%>" /><br />
                </td>
              </tr>
            </ItemTemplate>
          </asp:ListView>
        </div>

    </form>
</body>
</html>

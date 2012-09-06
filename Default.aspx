<%-- =====COPYRIGHT===== --%>
<%-- Copyright 2007 - 2012 Service Repair Solutions, Inc. --%>
<%-- =====COPYRIGHT===== --%>
<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Srs.WebPlatform.WebServices.Printer.Default" %>

<html>
<head>
    <title></title>
    <script src="jquery-1.4.2.min.js"></script>
    <script language="javascript">
        function MakePage(url) {
            return "<html>\n" +
                "<hea" + "d>\n" +
                "<tit" + "le>Temporary Printing Window</tit" + "le>\n" +
                "<scr" + "ipt type='text/javascript'>\n" +
                "func" + "tion step1() {\n" +
                "  setTimeout('step2()', 10);\n" +
                "}\n" +
                "func" + "tion step2() {\n" +
                "  window.print();\n" +
                "  window.close();\n" +
                "}\n" +
                "</scr" + "ipt>\n" +
                "</hea" + "d>\n" +
                "<body onLoad='step1()'>\n" +
                "<img width=\"680px\" src='" + url + "'/>\n" +
                "</body>\n" +
                "</html>\n";
        }
        function OnPrint(url) {
            var link = "about:blank";
            var stream = MakePage(url);
            var printWindow = window.open(link, "NewImagePrintWindow");
            printWindow.document.open();
            printWindow.document.write(stream);
            printWindow.document.close();
        }
        function checkPdfExtension(textBoxUrlId) {
            var uri = document.getElementById(textBoxUrlId);
            var url = uri.value;
            var index = url.lastIndexOf(".");
            var extension = url.substring(index + 1, url.length);
            if (extension.toLowerCase() == "pdf") {
                return true;
            }
            else { 
            alert('Url must contain pdf file');
            return false; }
            return true;
        }
        function PerformPrintAction() {
        // Get the iframe and print content
        var targetIframe = document.getElementById('iframePrinter');
        var printContent = $('#printerPreview').html();

        // Build the html document and append it into the iframe
        if (targetIframe != null && printContent != null) {
            var htmlDocument = '<html><head><title></title></head>';
            htmlDocument += '<body onload=\'this.focus(); this.print();\'>' + printContent + '</body></html>';
            var oDoc = (targetIframe.contentWindow || targetIframe.contentDocument);
            if (oDoc.document) oDoc = oDoc.document;
            oDoc.write(htmlDocument);
            oDoc.close();
        }
        else
            self.print();

    }
    </script>
</head>
<body>
<form runat="server" id="form1">
    <p>
        You can find the Printer Service: <a href="/WebServices/Srs.WebPlatform.WebServices.Printer.svc">SRS Printer 1.0</a><br/>
        You can check HeartBeat of Printer Serives: <a href="/WebServices/Srs.WebPlatform.WebServices.HeartBeat.svc/pox/IHeartBeatMonitoringServiceV1/HeartBeat">HeartBeatMonitoring</a>
    </p>
    <table>
    <tr>
    <td>Url</td>
    <td>
    <asp:TextBox ID="textBoxUrl" runat="server" Width="250px">http://www.google.com</asp:TextBox>
    </td>
    </tr>
    <tr>
    <td>Image Format</td>
    <td>
        <asp:DropDownList ID="dropDownListFormat" runat="server">
            <asp:ListItem Value="png">PNG</asp:ListItem>
            <asp:ListItem Value="jpg">JPEG</asp:ListItem>
            <asp:ListItem Value="gif">GIF</asp:ListItem>
        </asp:DropDownList>
    </td>
    </tr>
     <tr>
        <td>Margin Left</td>
        <td>
            <asp:TextBox ID="textBoxMarginLeft" runat="server" Text="0"></asp:TextBox>
         </td>
    </tr>
     <tr>
        <td>Margin Top</td>
        <td>
            <asp:TextBox ID="textBoxMarginTop" runat="server" Text="0"></asp:TextBox>
         </td>
    </tr>
    <tr>
        <td></td>
        <td>
            <asp:Button ID="buttonExportImage" runat="server" Text="Export Image" 
                onclick="ButtonExportImage_Click" />&nbsp;
            <asp:Button ID="buttonExportPDF" runat="server" Text="Export PDF" 
                onclick="ButtonExportPdf_Click" />
        </td>
    </tr>
    </table>
    
    <table width="100%" height="100%">
        <tr>
        <td valign="top">
            <input id="Button1" type="button" value="Print All" onclick="alert('');PerformPrintAction();" /></td>
    </tr>

    <tr valign="top">
        <td>
           <div id="printerPreview"> <asp:Literal ID="literalHttpStatusMessage" runat="server"></asp:Literal></div>
        </td>
    </tr>
        <tr valign="top">
            <td align="left" valign="top"><asp:Literal ID="literalViewer" runat="server"></asp:Literal></td>
        </tr>
    </table>
    <iframe id="iframePrinter" style="display:none"></iframe>
    </form>
    
    </body>
</html>

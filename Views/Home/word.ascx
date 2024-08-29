<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script src="../../Scripts/jquery-1.7.1.js" type="text/javascript"></script>
<script type="text/javascript">
    $(document).ready(function () {
        document.all.OA1.Open("http://localhost:21672/s.doc");
        $('#save').click(function () {
            //document.all.OA1.save();
            document.OA1.SetAppFocus();
            document.OA1.HttpInit();
            var sFileName = document.OA1.GetDocumentName();
            alert(sFileName);
            document.OA1.HttpAddPostOpenedFile(sFileName);

        });
    });
</script>
<object classid="clsid:7677E74E-5831-4C9E-A2DD-9B1EF9DF2DB4" id="OA1" 
    width="100%" height="520px" codebase="../../officeviewer.ocx">			
			<param name="Toolbars" value="-1">			
			<param name="BorderColor" value="15647136">
			<param name="BorderStyle" value="2">
		</object>

        


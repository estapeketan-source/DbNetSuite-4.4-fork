var dataSource

//////////////////////////////////////////////////////////////////////////////////////////////
function saveDataSource(grid, args)
//////////////////////////////////////////////////////////////////////////////////////////////
{
	var file = null
	
	var WshShell = new ActiveXObject("WScript.Shell")
	dataSource = WshShell.SpecialFolders("MyDocuments") + "\\My Data Sources";
	
	var fso = new ActiveXObject("Scripting.FileSystemObject")

	if (!fso.folderExists(dataSource))
		fso.createFolder(dataSource);
	
	dataSource += "\\dbnetgrid.htm";
	
    jQuery("#messageLine").html("Data source:[ <b>" + dataSource + "</b> ] created");
		
	try
	{
		file = fso.CreateTextFile( dataSource, true, true )
	}
	catch(ex)
	{
		alert( "Unable to create data source [" + dataSource + "]" )
		window.close()
		return
	}

	file.Write( args.response["html"] ) 
	file.close()
	
	var fileName = null

	if (args.response["mailMergeDocument"])
	{
		if (args.response["mergeDocumentExists"])
			fileName = args.response["mailMergeDocument"];
		else
			alert("Merge document [" + args.response["mailMergeDocument"] + "] not found");
	}
	
	runMailMerge(fileName);
}




/////////////////////////////////////////////////////////////////////////////////////////////
function runMailMerge(documentName)
//////////////////////////////////////////////////////////////////////////////////////////////
{
	var word;

	try
	{
		word = new ActiveXObject("Word.Application");
	}
	catch(e)
	{
		alert( "Word not found" );
		return;
	}
	
	
	if (!documentName)
		word.Documents.Add();
	else
	{
		try
		{
			word.Documents.Open(documentName);
		}
		catch(e)
		{
			alert( "Document: [" + documentName + "] not found" );
			return;
		}
	}

	try
	{
		var mailMerge = word.Application.ActiveDocument.MailMerge;
		mailMerge.OpenDataSource(dataSource);
		
		if( documentName )
		{
			mailMerge.Execute();
			window.close();
		}
	}
	catch(e)
	{
		alert(e.description)
		return;
	}
	
	word.Visible = true; 
	word.WindowState = 2; //Minimize
	word.WindowState = 1; //Maximize
}

//////////////////////////////////////////////////
function createChart(documentName)
//////////////////////////////////////////////////
{
	var excel, book, sheet, chart;
	
	try
	{
		excel = new ActiveXObject("Excel.Application"); 
	}
	catch(e)
	{
		alert( getText( "excelNotFound" ) );
		return;
	}

	excel.Visible = true;

	if (!documentName)
		book = excel.Workbooks.Add( xlWBATWorksheet );
	else
	{
		try
		{
			book = excel.Workbooks.Open(documentName);
		}
		catch(e)
		{
			alert( getText( "documentNotFound" ).replace("{document}", documentName) );
			return;
		}
	}

	try
	{
		if ( typeof( documentName ) == "undefined" )
		{
			sheet = book.Worksheets.Add();
			sheet.Name = "DbNetGrid Data";
			book.Worksheets("Sheet1").Delete;
		}
		else
		{
			excel.DisplayAlerts = false;
			book.Worksheets("DbNetGrid Data").Delete;
			excel.DisplayAlerts = true;

			sheet = book.Worksheets.Add();
			sheet.Name = "DbNetGrid Data";
		}

		window.frames["clipboard"].document.body.createTextRange().execCommand("copy");

		sheet.Range("A1").Select();
		sheet.Paste();
		sheet.Cells.Select;

		if ( book.Charts.Count > 0 )
			chart = book.Charts(1);
		else
			chart = book.Charts.Add();

		var plotBy = xlColumns;

		if ( queryString["plotby"] == "rows" )
			plotBy = xlRows;

		chart.SetSourceData( sheet.Range("A1:" + rangeEnd() ), plotBy );

		chart.Activate();	
	
	}
	catch(e)
	{
		alert( e.description );
	}

	window.close();
}

/////////////////////////////////////////////////
function rangeEnd()
/////////////////////////////////////////////////
{
	var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	var endRange = "";
	
	var cellCount = dataTable.rows[0].cells.length;

	if ( cellCount <= 26 )
		endRange = letters.substring( cellCount-1, cellCount );
	else
	{
		var multiple = (cellCount % 26);
		endRange = letters.substring( multiple-1, multiple ) + letters.substring( cellCount-1, cellCount );
	}

	endRange += dataTable.rows.length;

	return endRange;
}

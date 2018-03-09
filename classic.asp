<%
On Error Resume Next
volusionService = "http://store.mysite.com/net/WebService.aspx?Login=me@myemail.com&EncryptedPassword=E84BABEC568148C80855C873CCAE3E7C116FC4A5886EF17810183C2E4C4636EF&EDI_Name=Generic\Products&SELECT_Columns=p.ProductCode,p.ProductName,pd.ProductDescription"

Set xmlHttp = Server.CreateObject("Msxml2.serverXmlHttp")
xmlHttp.Open "GET", volusionService, false
xmlHttp.setRequestHeader "Content-Type", "text/xml; charset=utf-8"
xmlHttp.setRequestHeader "Content-Action", "Volusion_API"
xmlHttp.send
If err.number <> 0 then 
    Response.Write( "Store is not available." )
   Response.Write( "</br>" )
'    Response.Write( " Error: " & err.number & " | (0x: " & hex(err.number) & ") | Description: " & err.description & " | Source: " & err.source )
'    Response.Write( "</br>" )
'    Response.Write( "HTTP Status: " & xmlHttp.Status & " | Status Text: " & xmlHttp.StatusText)
'    Response.Write( "</br>" )
'    Response.Write(  xmlHttp.getAllResponseHeaders)
    wscript.echo Line
    Err.clear
    wscript.quit
Else
volusionResponse = xmlHttp.responseText
Set domDocument = Server.CreateObject("MSXML2.DOMDocument")
domDocument.async = false
domDocument.LoadXml(volusionResponse)
Set xmlHttp = Nothing
Set oProducts = domDocument.getElementsByTagName("Products")

Dim productArray()
ReDim productArray(-1)
dim ptext,pcode,pid,pname,pdesc

For each pNodeItem in oProducts
   for each cNodeItem in pNodeItem.ChildNodes
        if cNodeItem.nodeName = "ProductCode" then pcode = cNodeItem.text
        if cNodeItem.nodeName = "ProductID" then pid = cNodeItem.text
        if cNodeItem.nodeName = "ProductName" then pname = cNodeItem.text
        if cNodeItem.nodeName = "ProductDescription" then pdesc = cNodeItem.text
        ptext = pname &"|"& pdesc &"|"& pcode &"|"& pid
   next
    pdate = left(pname,8)
    tdate = year(date) & right("0" & month(date),2) & right("0" & day(date),2)
    if pdate >= tdate then
        ReDim Preserve productArray(UBound(productArray) + 1)
        productArray(UBound(productArray)) = ptext
    end if
Next

Set domDocument = Nothing

for a = UBound(productArray) - 1 To 0 Step -1
    for j= 0 to a
        if productArray(j)>productArray(j+1) then
            temp=productArray(j+1)
            productArray(j+1)=productArray(j)
            productArray(j)=temp
        end if
    next
next

if UBound(productArray) = -1 then
    Response.Write "No products found."
end if
For k=0 To UBound(productArray)
    product=Split(productArray(k),"|")
    link= "<strong><a href=""https://store.mysite.com/ProductDetails.asp"">" & product(0) & "</a></strong></br>" & product(1)
    Response.Write(link & "<br />")
Next

Erase productArray
End if
%>

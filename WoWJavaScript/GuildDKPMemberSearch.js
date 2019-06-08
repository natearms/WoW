function getMember() {

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/contacts?$select=lastname,wowc_class,wowc_totalep,wowc_totalgp,wowc_totalpr,wowc_trialend,wowc_trialstart&$filter=contains(lastname, 'oxnul') or contains(lastname, 'raume') or contains(lastname, 'randy')", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                for (var i = 0; i < results.value.length; i++) {
                    var lastname = results.value[i]["lastname"];
                    var wowc_class = results.value[i]["wowc_class"];
                    var wowc_totalep = results.value[i]["wowc_totalep"];
                    var wowc_totalgp = results.value[i]["wowc_totalgp"];
                    var wowc_totalpr = results.value[i]["wowc_totalpr"];
                    var wowc_trialend = results.value[i]["wowc_trialend"];
                    var wowc_trialstart = results.value[i]["wowc_trialstart"];
                }
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
}

function reference() {
    //check if document is loaded or not
    var imgControl = document.createElement(“IMG”);
//Check if documented loaded fully
document.onreadystatechange = function () {
    if (document.readyState == “complete”) {
    getnotesImages();
}
}
}

//this function is used to get image from notes
function getnotesImages() { //get regarding object id
    var regardingObjectId = window.parent.Xrm.Page.data.entity.getId();
    //assign notes entity name
    var entitySchemaName =”Annotation”;
    var odataQuery = “?$top = 1 & $select=AnnotationId, DocumentBody, MimeType&” +
    “$filter = ObjectId / Id eq guid'” + regardingObjectId +
    “‘ and IsDocument eq true and startswith(MimeType, ’image /’) “;
//call retrieveMultipleRecords method in SDK.REST javascript script library
SDK.REST.retrieveMultipleRecords(entitySchemaName, odataQuery, getnotesImagesCallback, function (error) { alert(error.message); }, function () { });
}
//process callbanck result
function getnotesImagesCallback(resultSet) {
    if (resultSet.length > 0) {
        var mimeType = resultSet[0].MimeType;
        var body = resultSet[0].DocumentBody;
        imgControl.src =”data: ” + mimeType + “; base64, ” + body;
        document.getElementById(‘imagediv’).appendChild(imgControl);
}
}

var bountyTotal = 1;
function getBankBounty() {
    var lootId = Xrm.Page.getAttribute("wowc_item").getValue();

    if (lootId == null) {

    } else {
        var lookupId = Xrm.Page.getAttribute("wowc_item").getValue()[0].id.replace("{", "").replace("}", "");
        
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/wowc_guildbankrecords?$select=wowc_highneed&$filter=_wowc_item_value eq " + lookupId + "", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null; 
                if (this.status === 200) {
                    var results = JSON.parse(this.response);
                    for (var i = 0; i < results.value.length; i++) {
                        var wowc_highneed = results.value[i]["wowc_highneed"];
                        var wowc_highneed_formatted = results.value[i]["wowc_highneed@OData.Community.Display.V1.FormattedValue"];
                        bountyTotal = wowc_highneed ? 2 : 1;
                    }
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    }
    
}
function setLookupItemFields() {
    var recordId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    var lootId = Xrm.Page.getAttribute("wowc_item").getValue();

    if (lootId == null) {
        Xrm.Page.getAttribute("wowc_eprate").setValue();
        Xrm.Page.getControl("wowc_eprate").setDisabled(false);
        Xrm.Page.getAttribute("wowc_category").setValue();
        Xrm.Page.getControl("wowc_category").setDisabled(false);
        
    }
    else {
        var lookupId = Xrm.Page.getAttribute("wowc_item").getValue()[0].id.replace("{", "").replace("}", "");

        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/wowc_loots(" + lookupId + ")?$select=wowc_epvalue,wowc_category", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    var wowc_epvalue = result["wowc_epvalue"];
                    var wowc_category = result["wowc_category"];
                    
                    if (wowc_category != null) {
                        Xrm.Page.getAttribute("wowc_category").setValue(wowc_category);
                        Xrm.Page.getControl("wowc_category").setDisabled(true);
                        
                    } else {
                        Xrm.Page.getControl("wowc_category").setDisabled(false);
                    }

                    if (wowc_epvalue != null) {
                        Xrm.Page.getAttribute("wowc_eprate").setValue(wowc_epvalue);
                        Xrm.Page.getControl("wowc_eprate").setDisabled(true);

                    } else {
                        Xrm.Page.getControl("wowc_eprate").setDisabled(false);
                    }
                    
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    }

}

function concatenateSubject() {
    var raidMember = Xrm.Page.getAttribute("wowc_raidmember").getValue();
    var itemName = Xrm.Page.getAttribute("wowc_item").getValue();

    if (raidMember != null && itemName != null) {
        var raidMember = Xrm.Page.getAttribute("wowc_raidmember").getValue()[0].name;
        var itemName = Xrm.Page.getAttribute("wowc_item").getValue()[0].name;
        Xrm.Page.getAttribute("subject").setValue(itemName + "-" + raidMember);
        Xrm.Page.getAttribute("subject").setSubmitMode("always");
    } else {
        Xrm.Page.getAttribute("subject").setValue("");
        Xrm.Page.getAttribute("subject").setSubmitMode("always");
    }
}
function updateItemInfoFromEP() {
    var epCategory = Xrm.Page.getAttribute("wowc_category").getValue();
    var epRate = Xrm.Page.getAttribute("wowc_eprate").getValue();
    var itemCategory = 0;
    var itemEP = 0;
    
    var lookupId = Xrm.Page.getAttribute("wowc_item").getValue()[0].id.replace("{", "").replace("}", "");

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/wowc_loots(" + lookupId + ")?$select=wowc_epvalue,wowc_category", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var result = JSON.parse(this.response);
                var wowc_epvalue = result["wowc_epvalue"];
                var wowc_category = result["wowc_category"];

                itemCategory = wowc_category;
                itemEP = wowc_epvalue;

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    if (itemCategory != epCategory || itemEP != epRate) {
        var entity = {};

        if (itemCategory != epCategory) {
            entity.wowc_category = epCategory;
        }
        if (itemEP != epRate) {
            entity.wowc_epvalue = epRate;
        }
        

        var req = new XMLHttpRequest();
        req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/wowc_loots(" + lookupId + ")", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 204) {
                    //Success - No Return Data - Do Something
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        
        req.send(JSON.stringify(entity));
        if (itemCategory != epCategory) {
            Xrm.Page.getControl("wowc_category").setDisabled(true);
            Xrm.Page.getControl("wowc_eprate").setDisabled(true);
        }
        
    }
}

function calculateEP() {
    var epRate = Xrm.Page.getAttribute("wowc_eprate").getValue();
    var epCount = Xrm.Page.getAttribute("wowc_epcount").getValue();
    var effortType = Xrm.Page.getAttribute("wowc_efforttype").getValue();
    var effortRate = 1;
    effortRate = effortType == 257260001? .20:1
    
    Xrm.Page.getAttribute("wowc_ep").setValue(((epRate * epCount)*effortRate)*bountyTotal);
    Xrm.Page.getAttribute("wowc_ep").setSubmitMode("always");
    Xrm.Page.getAttribute("wowc_overridevalues").setValue("0");
}
function overrideAEPValues() {
    var overrideCheckbox = Xrm.Page.getAttribute("wowc_overridevalues").getValue();

    if (overrideCheckbox == 1) {
        Xrm.Page.getControl("wowc_eprate").setDisabled(false);
        Xrm.Page.getControl("wowc_category").setDisabled(false);
    }
    else {
        Xrm.Page.getControl("wowc_eprate").setDisabled(true);
        Xrm.Page.getControl("wowc_category").setDisabled(true);
    }
}
function lockItemFieldForDonations() {
    
    var effortType = Xrm.Page.getAttribute("wowc_efforttype").getValue();

    if (effortType == "257260003") {
        Xrm.Page.getControl("wowc_item").setDisabled(true);
        Xrm.Page.getControl("wowc_raidmember").setDisabled(true);
    } else {
        Xrm.Page.getControl("wowc_item").setDisabled(false);
        Xrm.Page.getControl("wowc_raidmember").setDisabled(false);
    }
}
function lockFieldsForDonationsOnLoad() {
    var effortType = Xrm.Page.getAttribute("wowc_efforttype").getValue();
    if (effortType == "257260003") {
        Xrm.Page.getControl("wowc_item").setDisabled(true);
        Xrm.Page.getControl("wowc_raidmember").setDisabled(true);
        Xrm.Page.getControl("wowc_efforttype").setDisabled(true); 
    } else {
        Xrm.Page.getControl("wowc_item").setDisabled(false);
        Xrm.Page.getControl("wowc_raidmember").setDisabled(false);
        Xrm.Page.getControl("wowc_efforttype").setDisabled(false);
    }
    
}
function roundNumberValidationOnSave(context) {
    var effortType = Xrm.Page.getAttribute("wowc_efforttype").getValue();
    var epValue = Xrm.Page.getAttribute("wowc_epcount").getValue();

    var saveEvent = context.getEventArgs();

    if (effortType == "257260003" && epValue % 1 != 0) {
        Xrm.Page.ui.setFormNotification("Donations and Widthdrawls require a whole number in the EP Count field","ERROR")
        saveEvent.preventDefault();
    } 
}
function roundNumberValidation() {
    var effortType = Xrm.Page.getAttribute("wowc_efforttype").getValue();
    var epValue = Xrm.Page.getAttribute("wowc_epcount").getValue();

    if (effortType == "257260003" && epValue % 1 != 0) {
        Xrm.Page.ui.setFormNotification("Donations and Widthdrawls require a whole number in the EP Count field", "ERROR")
        
    }
}
var bountyTotal = 1;
var bountyMulti = 1;
function getBankBounty() {
    var lootId = Xrm.Page.getAttribute("wowc_item").getValue();

    if (lootId == null) {

    } else {
        var lookupId = Xrm.Page.getAttribute("wowc_item").getValue()[0].id.replace("{", "").replace("}", "");
        
        Xrm.WebApi.online.retrieveMultipleRecords("wowc_guildbankrecord", "?$select=wowc_highneed&$filter=_wowc_item_value eq "+lookupId+"").then(
            function success(results) {
                for (var i = 0; i < results.entities.length; i++) {
                    var wowc_highneed = results.entities[i]["wowc_highneed"];
                    var wowc_highneed_formatted = results.entities[i]["wowc_highneed@OData.Community.Display.V1.FormattedValue"];
                    bountyTotal = wowc_highneed ? 2 : 1;
                    bountyMulti = wowc_highneed ? 1.5 : 1;
                }
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );
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
       
        Xrm.WebApi.online.retrieveRecord("wowc_loot", lookupId, "?$select=wowc_category,wowc_epvalue").then(
            function success(result) {
                var wowc_category = result["wowc_category"];
                var wowc_epvalue = result["wowc_epvalue"];
                
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
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );
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
    
    Xrm.WebApi.online.retrieveRecord("wowc_loot", lookupId, "?$select=wowc_category,wowc_epvalue").then(
        function success(result) {
            var wowc_category = result["wowc_category"];
            var wowc_epvalue = result["wowc_epvalue"];
            
            itemCategory = wowc_category;
            itemEP = wowc_epvalue;
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );

    if (itemCategory != epCategory || itemEP != epRate) {
       
        var entity = {};
        if (itemCategory != epCategory) {
            entity.wowc_category = epCategory;
        }
        if (itemEP != epRate) {
            entity.wowc_epvalue = epRate;
        }

        Xrm.WebApi.online.updateRecord("wowc_loot", lookupId, entity).then(
            function success(result) {
                var updatedEntityId = result.id;
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );

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
    var highNeedFlag = Xrm.Page.getAttribute("wowc_highneedflag").getValue();
    var createdOn = Xrm.Page.getAttribute("wowc_created").getValue();
    
    if (highNeedFlag == true && bountyTotal == 1 && createdOn == true)
    {
       bountyTotal = 2;
       bountyMulti = 1.5;
	}     
    else if (highNeedFlag == true && bountyTotal == 2 && createdOn == true)
    {
       bountyTotal = 2;
       bountyMulti = 1.5;
	}
    else if (highNeedFlag == false && bountyTotal == 2 && createdOn == true)
    {
       bountyTotal = 1;
       bountyMulti = 1;
	}
    else if (highNeedFlag == false && bountyTotal == 1 && createdOn == true)
    {
       bountyTotal = 1;
       bountyMulti = 1;
	}

    if (bountyTotal == 2 && createdOn != true) {
        Xrm.Page.getAttribute("wowc_highneedflag").setValue(1);
        Xrm.Page.getAttribute("wowc_highneedflag").setSubmitMode("always");
    }
    else if (bountyTotal == 1 && createdOn != true) {
        Xrm.Page.getAttribute("wowc_highneedflag").setValue(0);
        Xrm.Page.getAttribute("wowc_highneedflag").setSubmitMode("always");
    }
    
    //effortRate = effortType == 257260001? .20:1
    if (epCount < 0 && effortType == 257260003) {
        Xrm.Page.getAttribute("wowc_ep").setValue(0);
        Xrm.Page.getAttribute("wowc_ep").setSubmitMode("always");
        Xrm.Page.getAttribute("wowc_overridevalues").setValue("0");
    } else {
        Xrm.Page.getAttribute("wowc_ep").setValue(((epRate * epCount) * effortRate) * bountyMulti);
        Xrm.Page.getAttribute("wowc_ep").setSubmitMode("always");
        Xrm.Page.getAttribute("wowc_overridevalues").setValue("0");
    }
    

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

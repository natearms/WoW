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
    var raidTeam = Xrm.Page.getAttribute("wowc_raidteam").getValue();
    var standbyTeam = Xrm.Page.getAttribute("wowc_standbyteam").getValue();
    var itemName = Xrm.Page.getAttribute("wowc_item").getValue();

    if (raidTeam != null && itemName != null && standbyTeam != null) {
        var raidTeam = Xrm.Page.getAttribute("wowc_raidteam").getValue()[0].name;
        var standbyTeam = Xrm.Page.getAttribute("wowc_standbyteam").getValue()[0].name;
        var itemName = Xrm.Page.getAttribute("wowc_item").getValue()[0].name;
        Xrm.Page.getAttribute("subject").setValue(itemName + "-" + raidTeam + "-" + standbyTeam);
        Xrm.Page.getAttribute("subject").setSubmitMode("always");
    }
    else if (raidTeam != null && itemName != null) {
        var raidTeam = Xrm.Page.getAttribute("wowc_raidteam").getValue()[0].name;
        var itemName = Xrm.Page.getAttribute("wowc_item").getValue()[0].name;
        Xrm.Page.getAttribute("subject").setValue(itemName + "-" + raidTeam);
        Xrm.Page.getAttribute("subject").setSubmitMode("always");
    }
    else {
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
    effortRate = effortType == 257260001 ? .20 : 1

    Xrm.Page.getAttribute("wowc_ep").setValue((epRate * epCount) * effortRate);
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
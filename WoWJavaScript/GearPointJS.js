function setLookupRaidMemberFields() {
    var recordId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    var raidMember = Xrm.Page.getAttribute("wowc_raidmember").getValue();

    if (raidMember == null) {
        Xrm.Page.getAttribute("wowc_class").setValue();
    }
    else {
        var lookupId = Xrm.Page.getAttribute("wowc_raidmember").getValue()[0].id.replace("{", "").replace("}", "");
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/contacts(" + lookupId + ")?$select=wowc_class", true);
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
                    var wowc_class = result["wowc_class"];
                    var wowc_class_formatted = result["wowc_class@OData.Community.Display.V1.FormattedValue"];
                    Xrm.Page.getAttribute("wowc_class").setValue(wowc_class);
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
        Xrm.Page.getAttribute("wowc_slot").setValue();
        Xrm.Page.getAttribute("wowc_rarity").setValue();
        Xrm.Page.getAttribute("wowc_ilvl").setValue();
        Xrm.Page.getAttribute("wowc_classspec").setValue();
    }
    else {
        var lookupId = Xrm.Page.getAttribute("wowc_item").getValue()[0].id.replace("{", "").replace("}", "");

        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/wowc_loots(" + lookupId + ")?$select=wowc_ilvl,wowc_rarity,wowc_slot, wowc_classspecmodifier", true);
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
                    var wowc_ilvl = result["wowc_ilvl"];
                    var wowc_ilvl_formatted = result["wowc_ilvl@OData.Community.Display.V1.FormattedValue"];
                    var wowc_rarity = result["wowc_rarity"];
                    var wowc_rarity_formatted = result["wowc_rarity@OData.Community.Display.V1.FormattedValue"];
                    var wowc_slot = result["wowc_slot"];
                    var wowc_slot_formatted = result["wowc_slot@OData.Community.Display.V1.FormattedValue"];
                    var wowc_classspecmodifier = result["wowc_classspecmodifier"];
                    var wowc_classspecmodifier_formatted = result["wowc_classspecmodifier@OData.Community.Display.V1.FormattedValue"];

                    Xrm.Page.getAttribute("wowc_slot").setValue(wowc_slot);
                    Xrm.Page.getAttribute("wowc_rarity").setValue(wowc_rarity);
                    Xrm.Page.getAttribute("wowc_ilvl").setValue(wowc_ilvl);
                    Xrm.Page.getAttribute("wowc_classspec").setValue(wowc_classspecmodifier);
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
        Xrm.Page.getAttribute("subject").setValue(raidMember + "-" + itemName);
        Xrm.Page.getAttribute("subject").setSubmitMode("always");
    } else {
        Xrm.Page.getAttribute("subject").setValue("");
        Xrm.Page.getAttribute("subject").setSubmitMode("always");
    }
}
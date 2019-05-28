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
        Xrm.Page.getAttribute("subject").setValue(raidMember + "-" + itemName);
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

    Xrm.Page.getAttribute("wowc_ep").setValue(epRate * epCount);
    Xrm.Page.getAttribute("wowc_ep").setSubmitMode("always");
    Xrm.Page.getAttribute("wowc_overridevalues").setValue("0");
}
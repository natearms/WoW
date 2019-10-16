function overrideValues() {
    var overrideValues = Xrm.Page.getAttribute("wowc_overridevalues").getValue();
    var overrideFields = ["wowc_name", "wowc_epvalue", "wowc_classspecmodifier", "wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_rarity", "wowc_slot", "wowc_category", "wowc_dropsfrom", "wowc_lootcouncil", "wowc_priority1", "wowc_priority2", "wowc_priority3", "wowc_marketrate", "wowc_marketratelastupdated"];

    if (overrideValues == 1) {
        for (var i = 0; i < overrideFields.length; i++) {
            Xrm.Page.getControl(overrideFields[i]).setDisabled(false);
        }
    }
    else {
        for (var i = 0; i < overrideFields.length; i++) {
            Xrm.Page.getControl(overrideFields[i]).setDisabled(true);
        }
    }
}
function fieldLockUnlock() {
    var name = Xrm.Page.getAttribute("wowc_name").getValue();
    var lockUnlockFields = ["wowc_name", "wowc_epvalue", "wowc_classspecmodifier", "wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_rarity", "wowc_slot", "wowc_category", "wowc_dropsfrom", "wowc_lootcouncil", "wowc_priority1", "wowc_priority2", "wowc_priority3", "wowc_marketrate", "wowc_marketratelastupdated"];

    if (name == null) {
        for (var i = 0; i < lockUnlockFields.length; i++) {
            Xrm.Page.getControl(lockUnlockFields[i]).setDisabled(false);
        }
    }
    else {
        for (var i = 0; i < lockUnlockFields.length; i++) {
            Xrm.Page.getControl(lockUnlockFields[i]).setDisabled(true);
        }
        Xrm.Page.getAttribute("wowc_overridevalues").setValue(false);
    }
}
function showHideFields() {
    var recordType = Xrm.Page.getAttribute("wowc_type").getValue();
    var setVisableFields = ["wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_classspecmodifier", "wowc_rarity", "wowc_slot", "wowc_defaultgp", "wowc_huntergpvalue", "wowc_tankgpvalue", "wowc_efforttype", "wowc_dropsfrom", "wowc_lootcouncil", "wowc_priority1", "wowc_priority2", "wowc_priority3", "wowc_marketrate", "wowc_marketratelastupdated"];
    var setRequiredFields = ["wowc_ilvl", "wowc_itemid", "wowc_itemidnum", "wowc_rarity", "wowc_slot", "wowc_efforttype"];
    var slotType = Xrm.Page.getAttribute("wowc_slot").getValue();

    if (recordType == 257260001 || recordType == 257260002) {
        
        for (var i = 0; i < setVisableFields.length; i++) {
            Xrm.Page.getControl(setVisableFields[i]).setVisible(false);
        }
        for (var i = 0; i < setRequiredFields.length; i++) {
            Xrm.Page.getAttribute(setRequiredFields[i]).setRequiredLevel("none");
        }
    }
    else {

        for (var i = 0; i < setVisableFields.length; i++) {
            Xrm.Page.getControl(setVisableFields[i]).setVisible(true);
        }
        for (var i = 0; i < setRequiredFields.length; i++) {
            Xrm.Page.getAttribute(setRequiredFields[i]).setRequiredLevel("required");
        }
        if (slotType != 257260008 && slotType != 257260009 && slotType != 257260007) {
            Xrm.Page.getAttribute("wowc_efforttype").setRequiredLevel("none");
        }
    }
}
function setEPFromMarketRate() {
    var marketRate = Xrm.Page.getAttribute("wowc_marketrate").getValue();
    var currentDateTime = new Date();
    
    if (marketRate != null) {

        Xrm.Page.getAttribute("wowc_epvalue").setValue(marketRate * .05);
        Xrm.Page.getAttribute("wowc_marketratelastupdated").setValue(currentDateTime);
        Xrm.Page.getAttribute("wowc_marketratelastupdated").setSubmitMode("always");

    }
    else {
        Xrm.Page.getAttribute("wowc_epvalue").setValue(null);
        Xrm.Page.getAttribute("wowc_marketratelastupdated").setValue(null);
        Xrm.Page.getAttribute("wowc_marketratelastupdated").setSubmitMode("always");
    }
}

function overrideValues() {
    var overrideValues = Xrm.Page.getAttribute("wowc_overridevalues").getValue();
    
    if (overrideValues == 1) {
        Xrm.Page.getControl("wowc_name").setDisabled(false);
        Xrm.Page.getControl("wowc_epvalue").setDisabled(false);
        Xrm.Page.getControl("wowc_classspecmodifier").setDisabled(false);
        Xrm.Page.getControl("wowc_ilvl").setDisabled(false);
        Xrm.Page.getControl("wowc_itemid").setDisabled(false);
        Xrm.Page.getControl("wowc_rarity").setDisabled(false);
        Xrm.Page.getControl("wowc_slot").setDisabled(false);
        Xrm.Page.getControl("wowc_category").setDisabled(false);
        
    }
    else {
        Xrm.Page.getControl("wowc_name").setDisabled(true);
        Xrm.Page.getControl("wowc_epvalue").setDisabled(true);
        Xrm.Page.getControl("wowc_classspecmodifier").setDisabled(true);
        Xrm.Page.getControl("wowc_ilvl").setDisabled(true);
        Xrm.Page.getControl("wowc_itemid").setDisabled(true);
        Xrm.Page.getControl("wowc_rarity").setDisabled(true);
        Xrm.Page.getControl("wowc_slot").setDisabled(true);
        Xrm.Page.getControl("wowc_category").setDisabled(true);
    }
}
function fieldLockUnlock() {
    var name = Xrm.Page.getAttribute("wowc_name").getValue();

    if (name == null) {
        Xrm.Page.getControl("wowc_name").setDisabled(false);
        Xrm.Page.getControl("wowc_epvalue").setDisabled(false);
        Xrm.Page.getControl("wowc_classspecmodifier").setDisabled(false);
        Xrm.Page.getControl("wowc_ilvl").setDisabled(false);
        Xrm.Page.getControl("wowc_itemid").setDisabled(false);
        Xrm.Page.getControl("wowc_rarity").setDisabled(false);
        Xrm.Page.getControl("wowc_slot").setDisabled(trufalsee);
        Xrm.Page.getControl("wowc_category").setDisabled(false);
        
    }
    else {
        Xrm.Page.getControl("wowc_name").setDisabled(true);
        Xrm.Page.getControl("wowc_epvalue").setDisabled(true);
        Xrm.Page.getControl("wowc_classspecmodifier").setDisabled(true);
        Xrm.Page.getControl("wowc_ilvl").setDisabled(true);
        Xrm.Page.getControl("wowc_itemid").setDisabled(true);
        Xrm.Page.getControl("wowc_rarity").setDisabled(true);
        Xrm.Page.getControl("wowc_slot").setDisabled(true);
        Xrm.Page.getControl("wowc_category").setDisabled(true);
        Xrm.Page.getAttribute("wowc_overridevalues").setValue(false);
    }

}

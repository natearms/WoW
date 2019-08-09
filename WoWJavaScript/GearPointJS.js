function setLookupRaidMemberFields() {
    var recordId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    var raidMember = Xrm.Page.getAttribute("wowc_raidmember").getValue();

    if (raidMember == null) {
        Xrm.Page.getAttribute("wowc_class").setValue();
    }
    else {
        var lookupId = Xrm.Page.getAttribute("wowc_raidmember").getValue()[0].id.replace("{", "").replace("}", "");
        Xrm.WebApi.online.retrieveRecord("contact", lookupId, "?$select=wowc_class").then(
            function success(result) {
                var wowc_class = result["wowc_class"];
                
                Xrm.Page.getAttribute("wowc_class").setValue(wowc_class);
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
        Xrm.Page.getAttribute("wowc_slot").setValue();
        Xrm.Page.getAttribute("wowc_rarity").setValue();
        Xrm.Page.getAttribute("wowc_ilvl").setValue();
        Xrm.Page.getAttribute("wowc_classspec").setValue();
        Xrm.Page.getAttribute("wowc_category").setValue();
        Xrm.Page.getControl("wowc_category").setDisabled(false);
        Xrm.Page.getAttribute("wowc_rarityvalue").setValue();
        Xrm.Page.getAttribute("wowc_slotmodifier").setValue();
        Xrm.Page.getAttribute("wowc_dropsfrom").setValue();
        Xrm.Page.getControl("wowc_dropsfrom").setDisabled(false);
    }
    else {
        var lookupId = Xrm.Page.getAttribute("wowc_item").getValue()[0].id.replace("{", "").replace("}", "");

        Xrm.WebApi.online.retrieveRecord("wowc_loot", lookupId, "?$select=wowc_category,wowc_classspecmodifier,wowc_ilvl,wowc_rarity,wowc_slot,_wowc_dropsfrom_value").then(
            function success(result) {
                var wowc_category = result["wowc_category"];
                var wowc_classspecmodifier = result["wowc_classspecmodifier"];
                var wowc_ilvl = result["wowc_ilvl"];
                var wowc_rarity = result["wowc_rarity"];
                var wowc_slot = result["wowc_slot"];
                var wowc_dropsfrom = result["_wowc_dropsfrom_value"];
                var setLookup = new Array();
                setLookup[0] = new Object();
                setLookup[0].id = result["_wowc_dropsfrom_value"];
                setLookup[0].name = result["_wowc_dropsfrom_value@OData.Community.Display.V1.FormattedValue"];
                setLookup[0].entityType = result["_wowc_dropsfrom_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                
                Xrm.Page.getAttribute("wowc_slot").setValue(wowc_slot);
                Xrm.Page.getAttribute("wowc_rarity").setValue(wowc_rarity);
                Xrm.Page.getAttribute("wowc_ilvl").setValue(wowc_ilvl);
                Xrm.Page.getAttribute("wowc_classspec").setValue(wowc_classspecmodifier);
                
                setSlotModifierFromItemChange(wowc_slot, wowc_classspecmodifier);
                setRarityValue(wowc_rarity);

                if (wowc_category != null) {
                    Xrm.Page.getAttribute("wowc_category").setValue(wowc_category);
                    Xrm.Page.getControl("wowc_category").setDisabled(true);
                } else {
                    Xrm.Page.getControl("wowc_category").setDisabled(false);
                }

                if (wowc_dropsfrom != null) {
                    Xrm.Page.getAttribute("wowc_dropsfrom").setValue(setLookup);
                    Xrm.Page.getControl("wowc_dropsfrom").setDisabled(true);
                } else {
                    Xrm.Page.getControl("wowc_dropsfrom").setDisabled(false);
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
function setMissingItemFields() {
    
    var gpCategory = Xrm.Page.getAttribute("wowc_category").getValue();
    var gpDropsFrom = Xrm.Page.getAttribute("wowc_dropsfrom").getValue();
    var gpDropsFromFormatted = gpDropsFrom[0].id.replace("{", "").replace("}", "");
    var itemCategory = "";
    var itemDropsFrom = "";
    
    var lookupId = Xrm.Page.getAttribute("wowc_item").getValue()[0].id.replace("{", "").replace("}", "");

    Xrm.WebApi.online.retrieveRecord("wowc_loot", lookupId, "?$select=wowc_category,_wowc_dropsfrom_value").then(
        function success(result) {
            itemCategory = result["wowc_category"];
            itemDropsFrom = results["_wowc_dropsfrom_value"];

        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );

    if (itemCategory != gpCategory) {
        
        var entity = {};
        entity.wowc_category = gpCategory;

        Xrm.WebApi.online.updateRecord("wowc_loot", lookupId, entity).then(
            function success(result) {
                var updatedEntityId = result.id;
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );
        Xrm.Page.getControl("wowc_category").setDisabled(true);
    }
    
    if (itemDropsFrom != gpDropsFromFormatted) {

        var entity = {};
        entity["wowc_DropsFrom@odata.bind"] = "/wowc_loots(" + gpDropsFromFormatted +")";
        
        Xrm.WebApi.online.updateRecord("wowc_loot", lookupId, entity).then(
            function success(result) {
                var updatedEntityId = result.id;
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );
        
        Xrm.Page.getControl("wowc_dropsfrom").setDisabled(true);
    }
    
    
}

function setSlotModifierFromItemChange(wowc_slot, wowc_classspec) {
    
    var lootId = Xrm.Page.getAttribute("wowc_item").getValue();
    var raiderClass = Xrm.Page.getAttribute("wowc_class").getValue();
    var itemSlot = wowc_slot;
    var classSpec = wowc_classspec; 

    setSlotModifier(lootId, raiderClass, itemSlot, classSpec);
}
function setSlotModifierFromClassSpecChange() {
    var lootId = Xrm.Page.getAttribute("wowc_item").getValue();
    var raiderClass = Xrm.Page.getAttribute("wowc_class").getValue();
    var itemSlot = Xrm.Page.getAttribute("wowc_slot").getValue();
    var classSpec = Xrm.Page.getAttribute("wowc_classspec").getValue();

    setSlotModifier(lootId, raiderClass, itemSlot, classSpec);
}
function setSlotModifier(wowc_lootid, wowc_class, wowc_slot, wowc_classspec) {

    if (wowc_lootid == null) {
        Xrm.Page.getAttribute("wowc_slotmodifier").setValue();
        
    } else {
        //Item Slot == 2H Weapon && Raider Class != Hunter
        if (wowc_slot == 257260000 && wowc_class != 257260001) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(2);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == 2H Weapon && Raider Class == Hunter
        else if (wowc_slot == 257260000 && wowc_class == 257260001) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(1);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == 1H Weapon && Raider Class != Hunter && Class/Spec Modifier != Tank with Shield
        else if (wowc_slot == 257260001 && wowc_class != 257260001 && wowc_classspec != 257260000) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(1.5);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == 1H Weapon && Raider Class == Hunter
        else if (wowc_slot == 257260001 && wowc_class == 257260001) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(.5);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == Head, Chest, Legs
        else if (wowc_slot == 257260002) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(1);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == Shoulder, Hands, Waist, Feet, Trinket
        else if (wowc_slot == 257260003) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(.75);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //(Item Slot == Wrist, Neck, Back, Finger, Off-hand, Wand, Relic, Bag || Item Slot == Sheild) && Class/Spec Modifier != Tank with Shield
        else if ((wowc_slot == 257260004 || wowc_slot == 257260005) && wowc_classspec != 257260000) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(.5);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == Ranged Weapon && Raider Class != Hunter
        else if (wowc_slot == 257260006 && wowc_class != 257260001) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(.5);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == Ranged Weapon && Raider Class == Hunter
        else if (wowc_slot == 257260006 && wowc_class == 257260001) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(1.5);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == 1H Weapon && Class Spec == Tank with Shield
        else if (wowc_slot == 257260001 && wowc_classspec == 257260000) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(.5);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        //Item Slot == Shield && Class Spec == Tank with Shield
        else if (wowc_slot == 257260005 && wowc_classspec == 257260000) {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(1.5);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
        else {
            Xrm.Page.getAttribute("wowc_slotmodifier").setValue(0);
            Xrm.Page.getAttribute("wowc_slotmodifier").setSubmitMode("always");
        }
    }
}
function setRarityValue(wowc_rarity) {
    
    var lootId = Xrm.Page.getAttribute("wowc_item").getValue();
    var rarityValue = wowc_rarity;

    if (lootId == null) {
        Xrm.Page.getAttribute("wowc_rarityvalue").setValue();
        //Xrm.Page.getControl("wowc_rarityvalue").setDisabled(false);
    } else {
        //Rarity == Green
        if (rarityValue == 257260000) {
            Xrm.Page.getAttribute("wowc_rarityvalue").setValue(2);
            Xrm.Page.getAttribute("wowc_rarityvalue").setSubmitMode("always");
        }
        //Rarity == Blue
        else if (rarityValue == 257260001) {
            Xrm.Page.getAttribute("wowc_rarityvalue").setValue(3);
            Xrm.Page.getAttribute("wowc_rarityvalue").setSubmitMode("always");
        }
        //Rarity == Epic
        else if (rarityValue == 257260002) {
            Xrm.Page.getAttribute("wowc_rarityvalue").setValue(4);
            Xrm.Page.getAttribute("wowc_rarityvalue").setSubmitMode("always");
        }
        //Rarity == Legendary
        else if (rarityValue == 257260003) {
            Xrm.Page.getAttribute("wowc_rarityvalue").setValue(5);
            Xrm.Page.getAttribute("wowc_rarityvalue").setSubmitMode("always");
        }
        //Rarity == Artifact
        else if (rarityValue == 257260005) {
            Xrm.Page.getAttribute("wowc_rarityvalue").setValue(6);
            Xrm.Page.getAttribute("wowc_rarityvalue").setSubmitMode("always");
        }
        //Default Rarity Value
        else {
            Xrm.Page.getAttribute("wowc_rarityvalue").setValue(0);
            Xrm.Page.getAttribute("wowc_rarityvalue").setSubmitMode("always");
        }
    }
}
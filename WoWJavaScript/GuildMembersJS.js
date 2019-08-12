function filterClassSpecilization() {
    var classOptionSet = Xrm.Page.getAttribute("wowc_class").getValue();
    var classSpecOptionSet = Xrm.Page.getControl("wowc_classspecialization");
    var secondaryClassSpecOptionSet = Xrm.Page.getControl("wowc_raidoffspecclassspecialization");
    var primaryClassSpec = Xrm.Page.getAttribute("wowc_classspecialization").getValue();
    var secondaryClassSpec = Xrm.Page.getAttribute("wowc_raidoffspecclassspecialization").getValue();
    var classSpecOptionSetValues = classSpecOptionSet.getAttribute().getOptions();
    var secondaryOptionSetValues = secondaryClassSpecOptionSet.getAttribute().getOptions();

    //clear optionset list and set previous values
    classSpecOptionSet.clearOptions();
    secondaryClassSpecOptionSet.clearOptions();
    Xrm.Page.getAttribute("wowc_classspecialization").setValue(primaryClassSpec);
    Xrm.Page.getAttribute("wowc_raidoffspecclassspecialization").setValue(secondaryClassSpec);
    
    // Guild Member Class = Druid
    if (classOptionSet == 257260000) {
        var filteredClassArray = [257260000, 257260001, 257260002];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Hunter
    else if (classOptionSet == 257260001) {
        var filteredClassArray = [257260003, 257260004, 257260005];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Mage
    else if (classOptionSet == 257260002) {
        var filteredClassArray = [257260006, 257260007, 257260008];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Paladin
    else if (classOptionSet == 257260003) {
        var filteredClassArray = [257260009, 257260010, 257260011];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Priest
    else if (classOptionSet == 257260004) {
        var filteredClassArray = [257260012, 257260013, 257260014];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Rogue
    else if (classOptionSet == 257260005) {
        var filteredClassArray = [257260015, 257260016, 257260017];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Shaman
    else if (classOptionSet == 257260006) {
        var filteredClassArray = [257260018, 257260019, 257260020];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Warlock
    else if (classOptionSet == 257260007) {
        var filteredClassArray = [257260021, 257260022, 257260023];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
    // Guild Member Class = Warrior
    else if (classOptionSet == 257260008) {
        var filteredClassArray = [257260024, 257260025, 257260026];
        optionSetFilterRoutine(filteredClassArray, classSpecOptionSetValues, classSpecOptionSet);
        optionSetFilterRoutine(filteredClassArray, secondaryOptionSetValues, secondaryClassSpecOptionSet);
    }
}
function optionSetFilterRoutine(filteredClassArray, optionSetValues, classSpecOptionSet) {

    for (var i = 0; i < filteredClassArray.length; i++) {
        for (var osv = 0; osv < optionSetValues.length; osv++) {
            if (filteredClassArray[i] == optionSetValues[osv].value) {
                classSpecOptionSet.addOption(optionSetValues[osv]);
                break;
            }
        }
    }
}
function filterClassSpecilization() {
    var primaryOptionSet = Xrm.Page.getAttribute("wowc_class").getValue();
    var optionSet = Xrm.Page.getControl("wowc_classspecialization");
    var optionSetValues = optionSet.getAttribute().getOptions();
    optionSet.clearOptions();
    
    // Guild Member Class = Druid
    if (primaryOptionSet == 257260000) {
        var filteredClassArray = [257260000, 257260001, 257260002];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);      
    }
    // Guild Member Class = Hunter
    else if (primaryOptionSet == 257260001) {
        var filteredClassArray = [257260003, 257260004, 257260005];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
    // Guild Member Class = Mage
    else if (primaryOptionSet == 257260002) {
        var filteredClassArray = [257260006, 257260007, 257260008];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
    // Guild Member Class = Paladin
    else if (primaryOptionSet == 257260003) {
        var filteredClassArray = [257260009, 257260010, 257260011];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
    // Guild Member Class = Priest
    else if (primaryOptionSet == 257260004) {
        var filteredClassArray = [257260012, 257260013, 257260014];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
    // Guild Member Class = Rogue
    else if (primaryOptionSet == 257260005) {
        var filteredClassArray = [257260015, 257260016, 257260017];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
    // Guild Member Class = Shaman
    else if (primaryOptionSet == 257260006) {
        var filteredClassArray = [257260018, 257260019, 257260020];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
    // Guild Member Class = Warlock
    else if (primaryOptionSet == 257260007) {
        var filteredClassArray = [257260021, 257260022, 257260023];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
    // Guild Member Class = Warrior
    else if (primaryOptionSet == 257260008) {
        var filteredClassArray = [257260024, 257260025, 257260026];
        optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet);
    }
}
function optionSetFilterRoutine(filteredClassArray, optionSetValues, optionSet) {

    for (var i = 0; i < filteredClassArray.length; i++) {
        for (var osv = 0; osv < optionSetValues.length; osv++) {
            if (filteredClassArray[i] == optionSetValues[osv].value) {
                optionSet.addOption(optionSetValues[osv]);
                break;
            }
        }
    }
}
StaticPopupDialogs["RAIDTOCSVDIALOG"] = {
    text = "Raid Members\npress ctrl-a then ctrl-c to copy",
    button1 = "Close",
    OnAccept = function()
        StaticPopup_Hide("RAIDTOCSVDIALOG");
    end,
    timeout = 0,
    whileDead = true,
    hideOnEscape = true,
    preferredIndex = 3,  -- avoid some UI taint, see http://www.wowace.com/announcements/how-to-avoid-some-ui-taint/
    OnShow = function (self, data)
        self.editBox:SetText(RaidMemberCSV)
    end,
    hasEditBox = true,
    editBoxWidth = 350,
    maxLetters=0,
  };
RaidMemberCSV = "Unable to get raid info";
SLASH_RAIDTOCSV1 = "/raidtocsv"
SLASH_RAIDTOCSV2 = "/rtc"
SlashCmdList["RAIDTOCSV"] = function(msg)
    RaidMemberCSV = "Unable to get raid info"
    members = GetNumGroupMembers()
    if members > 0 then
        raidMemberNames = {}
        for raidIndex = 0, members, 1
        do
            name, rank, subgroup, level, class, fileName, 
                zone, online, isDead, role, isML = GetRaidRosterInfo(raidIndex)
            raidMemberNames[raidIndex] = name
        end
        RaidMemberCSV = table.concat(raidMemberNames, ",")
        -- RaidToCSVFrame.frame.Text:SetText(raidCSV)
    end
    StaticPopup_Show("RAIDTOCSVDIALOG");
end
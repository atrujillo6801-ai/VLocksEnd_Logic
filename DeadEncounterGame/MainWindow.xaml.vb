Imports Newtonsoft.Json
Imports System.IO

' Simple class to hold save data (put this OUTSIDE the MainWindow class)
Public Class SaveData
    Public Property Name As String
    Public Property Health As Integer
    Public Property MaxHealth As Integer
    Public Property AttackPower As Integer
    Public Property Gold As Integer
    Public Property CurrentRoom As String
    Public Property Inventory As New List(Of String)
End Class
Class MainWindow

    Dim message As String = ""
    Dim currentRoom As Room 'we are declaring that whenever we see the container named currentRoom, it is holding something that was created from the blueprint class of Room.Notice that this is not creating something new, it's just telling the code what to read whatever is in the container "current room"
    Dim gameRooms As Dictionary(Of String, Room) ' We state that whenever we want to access a room, we will look it up in this dictionary by name. This allows us to easily manage multiple rooms and their connections.
    Dim player As Player ' Needed to be declared so that subroutines know what to read it as. In this case the container labeled "player" is holding something created from the blueprint class of Player. Notice that this is not a creation just a clarification of HOW to read that data as.



    Public Sub New()
        InitializeComponent()

        ' Create dictionary of rooms
        gameRooms = New Dictionary(Of String, Room)

        ' Create player
        player = New Player("Sam Stones")
        lblPlayerName.Content = player.Name

        'we are declaring that a room will be created from the blueprint class of room, and we will call it "entrance". 
        'We will then set the properties of this room (name, description, etc.from the blueprint properties) to create a unique location in our game world.]
        Dim entrance As New Room()
        entrance.Name = "West Entrance Hall"
        entrance.Description = "Dust fills the air. The hall is silent."
        entrance.Exits.Add("East", "East Dark Room")

        Dim eastRoom As New Room()
        eastRoom.Name = "East Dark Room"
        eastRoom.Description = "If only there was some kind of light switch."
        eastRoom.Exits.Add("West", "West Entrance Hall")
        eastRoom.Exits.Add("North", "North Zombie Room")
        eastRoom.Exits.Add("South", "South Key Room")

        Dim northRoom As New Room()
        northRoom.Name = "North Zombie Room"
        northRoom.Description = "What is that movement in the shadows?"
        northRoom.Exits.Add("South", "East Dark Room")



        Dim southRoom As New Room()
        southRoom.Name = "South Key Room"
        southRoom.Description = "A glint catches your eye from the corner of the room."
        southRoom.Exits.Add("North", "East Dark Room")

        northRoom.Enemy = New Enemy("Zombie", 30, 10)



        'this adds the rooms we created to the dictionary of rooms, using the string of the room's name as the key. This allows us to easily look up any room by its name later on (like when we want to move to a new room).
        gameRooms.Add(entrance.Name, entrance)
        gameRooms.Add(northRoom.Name, northRoom)
        gameRooms.Add(eastRoom.Name, eastRoom)
        gameRooms.Add(southRoom.Name, southRoom)



        ' Set starting room
        currentRoom = entrance

        ' Update the screen
        UpdateRoomDisplay()
        UpdateHealthBars()
        UpdateInventoryDisplay()
        AddToLog("Welcome to the dungeon.")
    End Sub


    'NewNavigation code from tutorial





    Private Sub LoadGame()
        Dim savePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\save.json")

        If Not File.Exists(savePath) Then
            AddToLog("No save file found. Starting new game.")
            Return
        End If

        Dim json As String = File.ReadAllText(savePath)
        Dim saveData As SaveData = JsonConvert.DeserializeObject(Of SaveData)(json)

        ' Restore the player from saved data
        player = New Player(saveData.Name)
        player.Health = saveData.Health
        player.MaxHealth = saveData.MaxHealth
        player.AttackPower = saveData.AttackPower
        player.Gold = saveData.Gold
        player.Inventory = saveData.Inventory

        ' Restore the room
        currentRoom = gameRooms(saveData.CurrentRoom)

        UpdateRoomDisplay()
        UpdateHealthBars()
        UpdateInventoryDisplay()
        AddToLog("Save loaded. Welcome back, " & player.Name & "!")
    End Sub



    Private Sub btnAttack_Click(sender As Object, e As RoutedEventArgs) Handles btnAttack.Click
        Dim enemy As Enemy = currentRoom.Enemy

        ' Player attacks first
        Dim playerDamage As Integer = player.Attack(enemy)
        AddToLog("You deal " & playerDamage & " damage to " & enemy.Name & "!")
        UpdateHealthBars()

        If Not enemy.IsAlive() Then
            AddToLog(enemy.Name & " has been defeated!")
            HandleEnemyDefeat(enemy)
            txtNpcDialogue.Text = "bleh"
            Return
        End If

        ' Enemy counter-attacks
        Dim enemyDamage As Integer = enemy.AttackPlayer(player)
        AddToLog(enemy.Name & " strikes back for " & enemyDamage & " damage!")
        UpdateHealthBars()

        If Not player.IsAlive() Then
            AddToLog("You have been defeated... Game Over.")
            ShowGameOver() ' needed to be declared
        End If
    End Sub


    Private Sub HandleEnemyDefeat(enemy As Enemy)
        If enemy.LootDrop <> "" Then
            player.PickUpItem(enemy.LootDrop)
            AddToLog("You found: " & enemy.LootDrop)
            UpdateInventoryDisplay() ' needed to be declared
        End If
        btnAttack.Visibility = Visibility.Collapsed
    End Sub

    ' Update the health bar visuals
    Private Sub UpdateHealthBars()
        ' Player health bar (a WPF ProgressBar named pbarPlayerHealth)
        pbarPlayerHealth.Value = player.Health
        pbarPlayerHealth.Maximum = player.MaxHealth
        lblPlayerHealth.Content = player.Health & " / " & player.MaxHealth

        ' Enemy health bar
        If currentRoom.Enemy IsNot Nothing Then
            pbarEnemyHealth.Value = Math.Max(0, currentRoom.Enemy.Health)
            pbarEnemyHealth.Maximum = currentRoom.Enemy.MaxHealth
        End If
    End Sub



    ' This runs when player clicks a direction button (e.g. "Go North")
    Private Sub AddToLog(message As String)
        txtCombatLog.AppendText(vbCrLf & message)
        txtCombatLog.ScrollToEnd()
    End Sub

    Private Sub btnNorth_Click(sender As Object, e As RoutedEventArgs) Handles btnNorth.Click
        ' Check if the current room has a "North" exit; notice it has to specifically contain it as a string!
        If currentRoom.Exits.ContainsKey("North") Then
            Dim nextRoomName As String = currentRoom.Exits("North")
            currentRoom = gameRooms(nextRoomName)  ' gameRooms is a Dictionary of all rooms, so HOW DO I DECLARE A DICTIONARY?
            UpdateRoomDisplay()
            AddToLog("You moved north into " & currentRoom.Name & ".")
        Else
            AddToLog("There is no path to the north.")
        End If
    End Sub

    Private Sub btnSouth_Click(sender As Object, e As RoutedEventArgs) Handles btnSouth.Click
        If currentRoom.Exits.ContainsKey("South") Then
            Dim nextRoomName As String = currentRoom.Exits("South")
            currentRoom = gameRooms(nextRoomName)
            UpdateRoomDisplay()
            AddToLog("You moved south into " & currentRoom.Name & ".")
        Else
            AddToLog("There is no path to the south.")
        End If
    End Sub

    Private Sub btnEast_Click(sender As Object, e As RoutedEventArgs) Handles btnEast.Click
        If currentRoom.Exits.ContainsKey("East") Then
            Dim nextRoomName As String = currentRoom.Exits("East")
            currentRoom = gameRooms(nextRoomName)
            UpdateRoomDisplay()
            AddToLog("You moved east into " & currentRoom.Name & ".")
        Else
            AddToLog("There is no path to the east.")
        End If
    End Sub

    Private Sub btnWest_Click(sender As Object, e As RoutedEventArgs) Handles btnWest.Click
        If currentRoom.Exits.ContainsKey("West") Then
            Dim nextRoomName As String = currentRoom.Exits("West")
            currentRoom = gameRooms(nextRoomName)
            UpdateRoomDisplay()
            AddToLog("You moved west into " & currentRoom.Name & ".")
        Else
            AddToLog("There is no path to the west.")
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        SaveGame()
    End Sub


    Private Sub btnTalkToNpc_Click(sender As Object, e As RoutedEventArgs) Handles btnTalkToNpc.Click
        If currentRoom.NpcName <> "" Then
            ' Show the dialogue panel (a Grid or StackPanel named pnlDialogue)
            pnlDialogue.Visibility = Visibility.Visible
            lblNpcName.Content = currentRoom.NpcName
            txtNpcDialogue.Text = currentRoom.NpcDialogue
        End If
    End Sub


    Private Sub btnCloseDialogue_Click(sender As Object, e As RoutedEventArgs)
        pnlDialogue.Visibility = Visibility.Collapsed
    End Sub

    Sub ShowGameOver() ' Had to declare from scratch. Used suggestion from VS
        MessageBox.Show("Game Over! Thanks for playing.")
        Application.Current.Shutdown()
    End Sub

    ' Helper: updates all UI elements to show the current room
    Private Sub UpdateRoomDisplay()
        lblRoomName.Content = currentRoom.Name
        txtRoomDescription.Text = currentRoom.Description 'there is no current text box for RoomDescription


        ' Show enemy/NPC/item status
        If currentRoom.Enemy IsNot Nothing AndAlso currentRoom.Enemy.IsAlive() Then
            lblEnemyStatus.Content = "Enemy present: " & currentRoom.Enemy.Name
            btnAttack.Visibility = Visibility.Visible
            txtNpcDialogue.Text = "RAHHHH" ' Clear any NPC dialogue if an enemy is present
        Else
            lblEnemyStatus.Content = "Room is clear."
            btnAttack.Visibility = Visibility.Collapsed
        End If
    End Sub


    Sub UpdateInventoryDisplay() ' Had to declare this as a subroutine so I can call it from other places (like when player picks up loot)
        lstInventory.Items.Clear()
        For Each item As String In player.Inventory
            lstInventory.Items.Add(item)
        Next
    End Sub




    Private Sub SaveGame()
        Dim saveData As New SaveData ' a simple data-transfer class (see below)
        saveData.Name = player.Name
        saveData.Health = player.Health
        saveData.MaxHealth = player.MaxHealth
        saveData.AttackPower = player.AttackPower
        saveData.Gold = player.Gold
        saveData.CurrentRoom = currentRoom.Name
        saveData.Inventory = player.Inventory

        Dim json As String = JsonConvert.SerializeObject(saveData, Formatting.Indented)
        Dim savePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\save.json")
        File.WriteAllText(savePath, json)
        AddToLog("Game saved successfully.")
    End Sub






End Class

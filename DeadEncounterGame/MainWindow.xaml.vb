' Add to the top of MainWindow.vb

Imports Newtonsoft.Json
Imports System.IO
Imports Microsoft.VisualBasic

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

    Dim currentRoom As Room
    Dim gameRooms As Dictionary(Of String, Room) ' Declare a dictionary to hold all rooms
    Dim player As Player ' Needed to be declared here so it can be accessed by all subroutines
    Dim lightsOn As Boolean = False

    Public Sub New()
        InitializeComponent()

        ' Create dictionary of rooms
        gameRooms = New Dictionary(Of String, Room)

        ' Create player
        player = New Player("Sam Stones")
        lblPlayerName.Content = player.Name

        Dim entrance As New Room()
        entrance.Name = "Entrance Hall"
        entrance.Description = "Dust fills the air. The hall is silent."

        Dim northRoom As New Room()
        northRoom.Name = "Dark Room"
        northRoom.Description = "The room is cold and almost completely dark."

        northRoom.Enemy = New Enemy("Zombie", 30, 10)

        entrance.Exits.Add("North", "Dark Room")
        northRoom.Exits.Add("South", "Entrance Hall")

        gameRooms.Add(entrance.Name, entrance)
        gameRooms.Add(northRoom.Name, northRoom)

        currentRoom = entrance

        ' Set starting room
        currentRoom = entrance

        ' Update the screen
        UpdateRoomDisplay()
        UpdateHealthBars()
        UpdateInventoryDisplay()
        AddToLog("Welcome to the dungeon.")
    End Sub

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
            Return
        End If

        ' Enemy counter-attacks
        Dim enemyDamage As Integer = enemy.AttackPlayer(player)
        AddToLog(enemy.Name & " strikes back for " & enemyDamage & " damage!")
        UpdateHealthBars()

        If Not player.IsAlive() Then
            AddToLog("You have been defeated... Game Over.")
            ShowGameOver()
        End If
    End Sub


    Private Sub HandleEnemyDefeat(enemy As Enemy)
        If enemy.LootDrop <> "" Then
            player.PickUpItem(enemy.LootDrop)
            AddToLog("You found: " & enemy.LootDrop)
            UpdateInventoryDisplay() ' needed to be declared
        End If

        UpdateRoomDisplay()
        UpdateHealthBars()
        AddToLog("The room is now clear.")
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
        If currentRoom.Exits.ContainsKey("North") Then
            Dim nextRoomName As String = currentRoom.Exits("North")
            currentRoom = gameRooms(nextRoomName)
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

    Private Sub btnLightSwitch_Click(sender As Object, e As RoutedEventArgs) Handles btnLightSwitch.Click
        lightsOn = Not lightsOn

        If lightsOn Then
            AddToLog("Lights turned ON")
        Else
            AddToLog("Lights turned OFF")
        End If

        UpdateRoomDisplay()
    End Sub


    Private Sub UpdateRoomDisplay()
        lblRoomName.Content = currentRoom.Name
        txtRoomDescription.Text = currentRoom.Description

        ' Change background based on lights
        If lightsOn Then
            imgRoom.Source = Nothing
            imgRoom.Source = New BitmapImage(New Uri("pack://application:,,,/Images/BreakerOn.png"))
        Else
            imgRoom.Source = Nothing
            imgRoom.Source = New BitmapImage(New Uri("pack://application:,,,/Images/BreakerOff.png"))
        End If

        ' Hides buttons for directions that don't exist in the current room
        btnNorth.Visibility = If(currentRoom.Exits.ContainsKey("North"), Visibility.Visible, Visibility.Collapsed)
        btnSouth.Visibility = If(currentRoom.Exits.ContainsKey("South"), Visibility.Visible, Visibility.Collapsed)
        btnEast.Visibility = If(currentRoom.Exits.ContainsKey("East"), Visibility.Visible, Visibility.Collapsed)
        btnWest.Visibility = If(currentRoom.Exits.ContainsKey("West"), Visibility.Visible, Visibility.Collapsed)

        ' Enemy stuff
        If currentRoom.Enemy IsNot Nothing AndAlso currentRoom.Enemy.IsAlive() Then
            lblEnemyStatus.Content = "Enemy: " & currentRoom.Enemy.Name
            btnAttack.Visibility = Visibility.Visible
            imgEnemy.Visibility = Visibility.Visible
            'placing player and enemy in combat 
            imgPlayer.HorizontalAlignment = HorizontalAlignment.Left
            imgPlayer.Margin = New Thickness(40, 0, 0, 40)

            imgEnemy.HorizontalAlignment = HorizontalAlignment.Right
            imgEnemy.Margin = New Thickness(0, 0, 40, 40)
        Else
            lblEnemyStatus.Content = "Room is clear."
            btnAttack.Visibility = Visibility.Collapsed
            imgEnemy.Visibility = Visibility.Collapsed

            imgPlayer.HorizontalAlignment = HorizontalAlignment.Center
            imgPlayer.Margin = New Thickness(0, 0, 0, 40)



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

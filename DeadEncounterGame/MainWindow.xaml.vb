' Add to the top of MainWindow.vb

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

    Dim currentRoom As Room
    Dim gameRooms As Dictionary(Of String, Room) ' Declare a dictionary to hold all rooms
    Dim player As Player ' Needed to be declared here so it can be accessed by all subroutines

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

    Private Sub btnAttack_Click(sender As Object, e As RoutedEventArgs)
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

    Private Sub btnNorth_Click(sender As Object, e As RoutedEventArgs)
        ' Check if the current room has a "North" exit
        If currentRoom.Exits.ContainsKey("North") Then
            Dim nextRoomName As String = currentRoom.Exits("North")
            currentRoom = gameRooms(nextRoomName)  ' gameRooms is a Dictionary of all rooms, so HOW DO I DECLARE A DICTIONARY?
            UpdateRoomDisplay()
        Else
            AddToLog("There is no path to the north.")
        End If
    End Sub

    Private Sub btnTalkToNpc_Click(sender As Object, e As RoutedEventArgs)
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

        ' Show/hide direction buttons based on available exits
        btnNorth.Visibility = If(currentRoom.Exits.ContainsKey("North"), Visibility.Visible, Visibility.Collapsed)
        btnSouth.Visibility = If(currentRoom.Exits.ContainsKey("South"), Visibility.Visible, Visibility.Collapsed)
        btnEast.Visibility = If(currentRoom.Exits.ContainsKey("East"), Visibility.Visible, Visibility.Collapsed)
        btnWest.Visibility = If(currentRoom.Exits.ContainsKey("West"), Visibility.Visible, Visibility.Collapsed)

        ' Show enemy/NPC/item status
        If currentRoom.Enemy IsNot Nothing AndAlso currentRoom.Enemy.IsAlive() Then
            lblEnemyStatus.Content = "Enemy present: " & currentRoom.Enemy.Name
            btnAttack.Visibility = Visibility.Visible
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

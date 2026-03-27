' Add to the top of MainWindow.vb

Imports Newtonsoft.Json
Imports System.IO


Class MainWindow

    Dim currentRoom As Room

    ' This runs when player clicks a direction button (e.g. "Go North")
    Private Sub AddToLog(message As String)
        txtCombatLog.AppendText(vbCrLf & message)
        txtCombatLog.ScrollToEnd()
    End Sub

    Private Sub btnNorth_Click(sender As Object, e As RoutedEventArgs)
        ' Check if the current room has a "North" exit
        If currentRoom.Exits.ContainsKey("North") Then
            Dim nextRoomName As String = currentRoom.Exits("North")
            currentRoom = gameRooms(nextRoomName)  ' gameRooms is a Dictionary of all rooms
            UpdateRoomDisplay()
        Else
            AddToLog("There is no path to the north.")
        End If
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














End Class

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Module Module1

    Private TcpListener As TcpListener
    Private SocketArray As New List(Of Socket)
    Private Is_Listening As Boolean = False
    Private PlayersCount As Integer

    Dim Users As New Dictionary(Of String, String)

    Sub Main()
        Console.Title = "CorrM Game Server"
        Console.CursorVisible = False
        Console.WindowHeight = 3
        Console.WindowWidth = 25

        ' UserName
        Users.Add("CorrM", "123456")

        TcpListener = New TcpListener(IPAddress.Any, 8188)
        TcpListener.Start()

        Dim Athread As New Thread(AddressOf Accept)
        Athread.Start()

        Console.Title = "CorrM Game Server || Port [8188]"

        Console.Write("Server State: ")
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("Online")

        Console.ResetColor()
        Console.Write("Player Count: ")
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("0")

        While True
            Console.SetCursorPosition(14, 1)
            Console.WriteLine(PlayersCount)
            Console.CursorVisible = False
            Thread.Sleep(50)
        End While

    End Sub

    Private Sub Accept()
        While True
            SocketArray.Add(TcpListener.AcceptSocket())
            PlayersCount += 1

            Dim Thread As New Thread(AddressOf Receive)
            Thread.Start(SocketArray.Count - 1)
        End While
    End Sub

    Private Sub Receive(SockID As Integer)

        While True
            Try
                Dim Buffers As Byte() = New Byte(1024) {}
                SocketArray(SockID).Receive(Buffers)

                Select Case Buffers(0)
                    Case 1
                        Dim BlockID As Byte = Buffers(1)

                        For I = 0 To SocketArray.Count - 1
                            If Not I = SockID Then SocketArray(I).Send(New Byte() {44, BlockID})
                        Next
                    Case 2
                        Debug.Write("Rceved Some Text: ")
                        Debug.WriteLine(String.Join(" ", Buffers))

                        Dim ReSendBuf As Byte() = Buffers
                        ReSendBuf(0) = 45

                        For I = 0 To SocketArray.Count - 1
                            If Not I = SockID Then SocketArray(I).Send(ReSendBuf)
                        Next

                    Case 3
                        Dim buf As New List(Of Byte)
                        buf.AddRange(Buffers)
                        buf.RemoveAt(0)

                        Dim UserPass As String = Text.Encoding.ASCII.GetString(buf.ToArray()).Replace(vbNullChar, "")

                        Dim UserName As String = Strings.Split(UserPass, Chr(3))(0)
                        Dim Password As String = Strings.Split(UserPass, Chr(3))(1)

                        Debug.WriteLine(String.Format("Check This User: {0} => {1}", UserName, Password))

                        Dim xPass As String = String.Empty
                        Dim GoodInfo As Boolean = False

                        If Users.TryGetValue(UserName, xPass) Then
                            If Password = xPass Then
                                GoodInfo = True
                            End If
                        End If

                        SocketArray(SockID).Send(New Byte() {46, IIf(GoodInfo, 1, 0)})
                End Select

            Catch ex As Exception
                Exit While
            End Try
        End While
        PlayersCount -= 1
    End Sub

End Module

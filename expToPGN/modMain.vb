Module modMain
    Public entete As String
    Public epd As String

    Sub Main()
        Dim chaine As String, tabChaine() As String, tabTmp() As String, i As Integer, j As Integer
        Dim fichierINI As String, fichierEXP As String, moteurEXP As String, moteur_court As String, fichierPGN As String
        Dim position As String, profMax As Integer, tabPGN() As String, departEPD As String, fichierUCI As String
        Dim coup As String, prof As Integer, maxCoups As Integer
        Dim tabPositions(1000000) As String, indexPosition As Integer, nbPGN As Integer, offsetPosition As Integer

        Console.Title = My.Computer.Name

        indexPosition = 0
        nbPGN = 0
        offsetPosition = 0
        fichierUCI = ""

        If My.Computer.FileSystem.GetFileInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "Documents\Visual Studio 2013\ProjectsexpToPGN\expToPGN\bin\Debug\expToPGN.exe").LastWriteTime > My.Computer.FileSystem.GetFileInfo(My.Application.Info.AssemblyName & ".exe").LastWriteTime Then
            MsgBox("Il existe une version plus récente de ce programme !", MsgBoxStyle.Information)
            End
        End If

        fichierINI = My.Computer.Name & ".ini"
        moteurEXP = "D:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\20T Eman 8.20 x64 PCNT.exe"
        fichierEXP = "D:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\Eman.exp"
        If My.Computer.Name = "BOIS" Or My.Computer.Name = "HTPC" Or My.Computer.Name = "TOUR-COURTOISIE" Then
            moteurEXP = "D:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\20T Eman 8.20 x64 BMI2.exe"
            fichierEXP = "D:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\Eman.exp"
        ElseIf My.Computer.Name = "BUREAU" Or My.Computer.Name = "WORKSTATION" Then
            moteurEXP = "E:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\20T Eman 8.20 x64 BMI2.exe"
            fichierEXP = "E:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\Eman.exp"
        End If

        If My.Computer.FileSystem.FileExists(fichierINI) Then
            chaine = My.Computer.FileSystem.ReadAllText(fichierINI)
            If chaine <> "" And InStr(chaine, vbCrLf) > 0 Then
                tabChaine = Split(chaine, vbCrLf)
                For i = 0 To UBound(tabChaine)
                    If tabChaine(i) <> "" And InStr(tabChaine(i), " = ") > 0 Then
                        tabTmp = Split(tabChaine(i), " = ")
                        If tabTmp(0) <> "" And tabTmp(1) <> "" Then
                            If InStr(tabTmp(1), "//") > 0 Then
                                tabTmp(1) = Trim(gauche(tabTmp(1), tabTmp(1).IndexOf("//") - 1))
                            End If
                            Select Case tabTmp(0)
                                Case "moteurEXP"
                                    moteurEXP = Replace(tabTmp(1), """", "")
                                Case "fichierEXP"
                                    fichierEXP = Replace(tabTmp(1), """", "")
                                Case Else

                            End Select
                        End If
                    End If
                Next
            End If
        End If
        My.Computer.FileSystem.WriteAllText(fichierINI, "moteurEXP = " & moteurEXP & vbCrLf _
                                                      & "fichierEXP = " & fichierEXP & vbCrLf, False)

        If Not expV2(fichierEXP) Then
            MsgBox(nomFichier(fichierEXP) & " <> experience format v2 !?", MsgBoxStyle.Exclamation)
            End
        End If

        profMax = 32
        Console.WriteLine("Max depth ? (5 or higher)")
        chaine = Console.ReadLine
        If IsNumeric(chaine) Then
            profMax = CInt(chaine)
        Else
            End
        End If
        ReDim tabPGN(profMax)

        maxCoups = 3
        Console.WriteLine(vbCrLf & "Max plies ?")
        chaine = Console.ReadLine
        If IsNumeric(chaine) Then
            maxCoups = CInt(chaine)
        Else
            End
        End If

        tabPositions(0) = ""
        departEPD = ""

        chaine = Replace(Command(), """", "")
        If chaine = "" Then
            Console.WriteLine(vbCrLf & "Which position ?")
            Console.WriteLine("(enter a UCI or leave blank for the default startpos)")
            tabPositions(0) = Trim(Console.ReadLine)
        ElseIf My.Computer.FileSystem.FileExists(chaine) Then
            fichierUCI = Replace(chaine, ".pgn", "_uci.pgn")
            If My.Computer.FileSystem.FileExists(fichierUCI) Then
                My.Computer.FileSystem.DeleteFile(fichierUCI)
            End If
            pgnUCI("pgn-extract.exe", chaine, "_uci")
            tabChaine = Split(My.Computer.FileSystem.ReadAllText(fichierUCI), vbCrLf)
            For i = 0 To UBound(tabChaine)
                If tabChaine(i) <> "" And InStr(tabChaine(i), "[") = 0 And InStr(tabChaine(i), "]") = 0 And InStr(tabChaine(i), "*") > 0 Then
                    tabPositions(0) = Trim(tabChaine(i).Substring(0, tabChaine(i).IndexOf("*")))
                    Exit For
                End If
            Next
        End If

        moteur_court = nomFichier(moteurEXP)
        Console.Write(vbCrLf & "Loading " & moteur_court & "... ")
        chargerMoteur(moteurEXP, fichierEXP)
        Console.WriteLine("OK" & vbCrLf)

        Console.WriteLine(entete)

        If fichierUCI = "" Then
            fichierPGN = "under_D" & profMax & ".pgn"
        Else
            fichierPGN = Replace(Replace(nomFichier(fichierUCI), "_uci.pgn", "") & "_under_D" & profMax & ".pgn", " ", "_")
        End If
        If My.Computer.FileSystem.FileExists(fichierPGN) Then
            My.Computer.FileSystem.DeleteFile(fichierPGN, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
        End If

        Console.Title = "searching in " & nomFichier(fichierEXP)
        Do
            position = tabPositions(indexPosition)
            chaine = expListe(position)
            If indexPosition = 0 Then
                If tabPositions(0) <> "" Then
                    departEPD = epd
                End If
            End If

            If chaine <> "" Then
                tabChaine = Split(chaine, vbCrLf)
                For i = 0 To UBound(tabChaine)
                    If tabChaine(i) <> "" And InStr(tabChaine(i), "mate", CompareMethod.Text) = 0 Then
                        tabTmp = Split(Replace(tabChaine(i), ":", ","), ",")

                        coup = Trim(tabTmp(1))
                        prof = CInt(Trim(tabTmp(3)))
                        chaine = Trim(position & " " & coup)

                        If (profMax = 5 And prof < profMax) _
                        Or (profMax > 5 And 4 < prof And prof < profMax) Then
                            If position <> "" Then
                                For j = 4 To profMax
                                    If InStr(tabPGN(j), position & ";", CompareMethod.Text) > 0 Then
                                        tabPGN(j) = Replace(tabPGN(j), position & ";", "")
                                        nbPGN = nbPGN - 1
                                    End If
                                Next
                            End If
                            tabPGN(prof) = tabPGN(prof) & chaine & ";"
                            nbPGN = nbPGN + 1
                        End If
                        offsetPosition = offsetPosition + 1
                        tabPositions(offsetPosition) = chaine

                        If offsetPosition Mod 50 = 0 Then
                            Console.Clear()
                            Console.Write("total : " & Format(nbPGN, "00 000 pgn"))
                            Console.WriteLine(" | " & Format(offsetPosition, "000 000 lines"))
                        End If
                    End If
                Next
            End If

            indexPosition = indexPosition + 1
        Loop While tabPositions(indexPosition) <> "" And nbCaracteres(tabPositions(indexPosition), " ") < (maxCoups - 1) And offsetPosition < 100000

        'décharger le moteur
        dechargerMoteur()

        Console.Clear()
        Console.Title = "writing to " & nomFichier(fichierPGN)
        chaine = ""
        If tabPositions(0) = "" Then
            chaine = "startpos"
        Else
            chaine = tabPositions(0)
        End If
        Console.WriteLine("From " & chaine & " (max D" & profMax & " / " & maxCoups & " moves) :")
        Console.WriteLine(StrDup(30, "-"))
        chaine = ""
        nbPGN = 0
        indexPosition = 0
        For prof = 4 To profMax
            If tabPGN(prof) <> "" Then
                j = nbCaracteres(tabPGN(prof), ";")
                nbPGN = nbPGN + j
                Console.Write("D" & Format(prof, "000") & " : " & Format(j, "00 000 pgn"))
                Console.WriteLine(" | " & Format(nbPGN, "00 000 pgn"))
                tabTmp = Split(tabPGN(prof), ";")
                For j = 0 To UBound(tabTmp)
                    If tabTmp(j) <> "" Then
                        indexPosition = indexPosition + 1
                        If departEPD <> "" Then
                            tabTmp(j) = Replace(tabTmp(j), tabPositions(0) & " ", "", , 1)
                        End If
                        My.Computer.FileSystem.WriteAllText(fichierPGN, structurePGN(tabTmp(j), departEPD, , , "D" & prof, indexPosition), True)
                    End If
                Next
            End If
        Next
        Console.Title = My.Computer.Name


        Console.WriteLine(StrDup(30, "-"))
        Console.WriteLine("total : " & Format(nbPGN, "00 000 pgn") & vbCrLf)

        If My.Computer.FileSystem.FileExists(fichierUCI) Then
            My.Computer.FileSystem.DeleteFile(fichierUCI)
        End If

        Console.WriteLine("Press ENTER to close the window.")
        Console.ReadLine()
    End Sub

End Module

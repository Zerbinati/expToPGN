Imports VB = Microsoft.VisualBasic

Module modFonctions
    Public processus As System.Diagnostics.Process
    Public entree As System.IO.StreamWriter
    Public sortie As System.IO.StreamReader

    Public Sub chargerMoteur(chemin As String, fichierEXP As String)
        Dim chaine As String

chargement_moteur:
        Try
            processus = New System.Diagnostics.Process()

            processus.StartInfo.RedirectStandardOutput = True
            processus.StartInfo.UseShellExecute = False
            processus.StartInfo.RedirectStandardInput = True
            processus.StartInfo.CreateNoWindow = True
            processus.StartInfo.WorkingDirectory = My.Application.Info.DirectoryPath
            processus.StartInfo.FileName = chemin
            processus.Start()
            processus.PriorityClass = 64 '64 (idle), 16384 (below normal), 32 (normal), 32768 (above normal), 128 (high), 256 (realtime)

            entree = processus.StandardInput
            sortie = processus.StandardOutput

            entree.WriteLine("uci")
            chaine = ""
            While InStr(chaine, "uciok") = 0
                If processus.HasExited Then
                    entree.Close()
                    sortie.Close()
                    processus.Close()
                    GoTo chargement_moteur
                End If
                chaine = sortie.ReadLine
                Threading.Thread.Sleep(10)
            End While

            entree.WriteLine("setoption name threads value 1")
            entree.WriteLine("setoption name Experience File value " & fichierEXP)

            entete = ""
            While entete = ""
                If processus.HasExited Then
                    entree.Close()
                    sortie.Close()
                    processus.Close()
                    GoTo chargement_moteur
                End If
                chaine = sortie.ReadLine
                If InStr(chaine, "info", CompareMethod.Text) > 0 _
                And InStr(chaine, "string", CompareMethod.Text) > 0 _
                And (InStr(chaine, "collision", CompareMethod.Text) > 0 Or InStr(chaine, "duplicate", CompareMethod.Text) > 0) Then
                    entete = Replace(chaine, fichierEXP, nomFichier(fichierEXP)) & vbCrLf
                End If
                Threading.Thread.Sleep(10)
            End While

            entree.WriteLine("isready")
            chaine = ""
            While InStr(chaine, "readyok") = 0
                If processus.HasExited Then
                    entree.Close()
                    sortie.Close()
                    processus.Close()
                    GoTo chargement_moteur
                End If
                chaine = sortie.ReadLine
                Threading.Thread.Sleep(10)
            End While
        Catch ex As Exception
            If processus.HasExited Then
                entree.Close()
                sortie.Close()
                processus.Close()
                GoTo chargement_moteur
            End If
        End Try

    End Sub

    Public Sub dechargerMoteur()
        entree.Close()
        sortie.Close()
        processus.Close()

        entree = Nothing
        sortie = Nothing
        processus = Nothing
    End Sub

    Public Function expListe(position As String) As String
        Dim chaine As String, ligne As String

        If position = "" Then
            entree.WriteLine("position startpos")
        ElseIf InStr(position, "/", CompareMethod.Text) > 0 _
          And (InStr(position, " w ", CompareMethod.Text) > 0 Or InStr(position, " b ", CompareMethod.Text) > 0) Then
            entree.WriteLine("position fen " & position)
        Else
            entree.WriteLine("position startpos moves " & position)
        End If
        entree.WriteLine("expex")

        entree.WriteLine("isready")

        chaine = ""
        ligne = ""
        While InStr(ligne, "readyok") = 0
            ligne = sortie.ReadLine
            If InStr(ligne, "Fen: ", CompareMethod.Text) > 0 Then
                epd = Trim(Replace(ligne, "Fen: ", ""))
            ElseIf InStr(ligne, "quality:") > 0 Then
                chaine = chaine & ligne & vbCrLf
            End If
        End While

        Return chaine
    End Function

    Public Function expV2(cheminEXP As String) As Boolean
        Dim lecture As IO.FileStream, tampon As Long, tabTampon() As Byte

        lecture = New IO.FileStream(cheminEXP, IO.FileMode.Open)

        'SugaR Experience version 2
        '0123456789abcdef0123456789
        tampon = 26
        ReDim tabTampon(tampon - 1)
        lecture.Read(tabTampon, 0, tampon)
        lecture.Close()

        If System.Text.Encoding.UTF8.GetString(tabTampon) <> "SugaR Experience version 2" Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Function gauche(texte As String, longueur As Integer) As String
        If longueur > 0 Then
            Return VB.Left(texte, longueur)
        Else
            Return ""
        End If
    End Function

    Public Function nbCaracteres(ByVal chaine As String, ByVal critere As String) As Integer
        Return Len(chaine) - Len(Replace(chaine, critere, ""))
    End Function

    Public Function nomFichier(chemin As String) As String
        Return My.Computer.FileSystem.GetName(chemin)
    End Function

    Public Sub pgnUCI(chemin As String, fichier As String, suffixe As String, Optional priorite As Integer = 64)
        Dim nom As String, commande As New Process()
        Dim dossierFichier As String, dossierTravail As String

        nom = Replace(nomFichier(fichier), ".pgn", "")

        dossierFichier = fichier.Substring(0, fichier.LastIndexOf("\"))
        dossierTravail = My.Computer.FileSystem.GetParentPath(chemin)

        'si pgn-extract.exe ne se trouve à l'emplacement prévu (par <nom_ordinateur>.ini)
        If Not My.Computer.FileSystem.FileExists(dossierTravail & "\pgn-extract.exe") Then

            'si pgn-extract.exe ne se trouve dans le même dossier que le notre application
            dossierTravail = Environment.CurrentDirectory
            If Not My.Computer.FileSystem.FileExists(dossierTravail & "\pgn-extract.exe") Then

                'on cherche s'il se trouve dans le même dossier que le fichierPGN
                dossierTravail = dossierFichier
                If Not My.Computer.FileSystem.FileExists(dossierTravail & "\pgn-extract.exe") Then

                    'pgn-extract.exe est introuvable
                    MsgBox("Veuillez copier pgn-extract.exe dans :" & vbCrLf & dossierTravail, MsgBoxStyle.Critical)
                    dossierTravail = Environment.CurrentDirectory
                    If Not My.Computer.FileSystem.FileExists(dossierTravail & "\pgn-extract.exe") Then
                        End
                    End If
                End If
            End If

        End If

        'si le fichierPGN ne se trouve pas dans le dossier de travail
        If dossierFichier <> dossierTravail Then
            'on recopie temporairement le fichierPGN dans le dossierTravail
            My.Computer.FileSystem.CopyFile(fichier, dossierTravail & "\" & nom & ".pgn", True)
        End If

        commande.StartInfo.FileName = dossierTravail & "\pgn-extract.exe"
        commande.StartInfo.WorkingDirectory = dossierTravail

        If InStr(nom, " ") = 0 Then
            commande.StartInfo.Arguments = " -s -Wuci -o" & nom & suffixe & ".pgn" & " " & nom & ".pgn"
        Else
            commande.StartInfo.Arguments = " -s -Wuci -o""" & nom & suffixe & ".pgn""" & " """ & nom & ".pgn"""
        End If

        commande.StartInfo.CreateNoWindow = True
        commande.StartInfo.UseShellExecute = False
        commande.Start()
        commande.PriorityClass = priorite '64 (idle), 16384 (below normal), 32 (normal), 32768 (above normal), 128 (high), 256 (realtime)
        commande.WaitForExit()

        'si le dossierTravail ne correspond pas au dossier du fichierPGN
        If dossierFichier <> dossierTravail Then
            'on déplace le fichier moteur
            Try
                My.Computer.FileSystem.DeleteFile(dossierTravail & "\" & nom & ".pgn")
            Catch ex As Exception

            End Try
            My.Computer.FileSystem.MoveFile(dossierTravail & "\" & nom & suffixe & ".pgn", dossierFichier & "\" & nom & suffixe & ".pgn")
        End If
    End Sub

    Public Function structurePGN(partie As String, Optional fen As String = "", Optional blanc As String = "?", Optional noir As String = "?", Optional site As String = "?", Optional round As String = "?") As String
        Dim chaine As String, tabChaine() As String

        chaine = "[Event ""Computer chess game""]" & vbCrLf _
               & "[Site """ & site & """]" & vbCrLf _
               & "[Date """ & Format(Now, "yyyy.MM.dd") & """]" & vbCrLf _
               & "[Round """ & round & """]" & vbCrLf _
               & "[White """ & blanc & """]" & vbCrLf _
               & "[Black """ & noir & """]" & vbCrLf

        If fen <> "" Then
            chaine = chaine & "[SetUp ""1""]" & vbCrLf _
                            & "[FEN """ & fen & """]" & vbCrLf
        End If

        If InStr(partie, "1/2-1/2", CompareMethod.Text) = 0 And InStr(partie, "#", CompareMethod.Text) = 0 _
         And InStr(partie, " 1-0", CompareMethod.Text) = 0 And InStr(partie, " 0-1", CompareMethod.Text) = 0 Then
            chaine = chaine & "[Result ""*""]" & vbCrLf & vbCrLf _
                            & Trim(partie) & " *" & vbCrLf & vbCrLf

        ElseIf InStr(partie, "1/2-1/2", CompareMethod.Text) > 0 Then
            chaine = chaine & "[Result ""1/2-1/2""]" & vbCrLf & vbCrLf _
                            & Trim(partie) & vbCrLf & vbCrLf

        ElseIf InStr(partie, " 1-0", CompareMethod.Text) > 0 Then
            chaine = chaine & "[Result ""1-0""]" & vbCrLf & vbCrLf _
                            & Trim(partie) & vbCrLf & vbCrLf

        ElseIf InStr(partie, " 0-1", CompareMethod.Text) > 0 Then
            chaine = chaine & "[Result ""0-1""]" & vbCrLf & vbCrLf _
                            & Trim(partie) & vbCrLf & vbCrLf

        ElseIf InStr(partie, "#", CompareMethod.Text) > 0 Then
            tabChaine = Split(Trim(partie), " ")
            If InStr(tabChaine(UBound(tabChaine) - 1), ".", CompareMethod.Text) = 0 Then
                chaine = chaine & "[Result ""0-1""]" & vbCrLf & vbCrLf _
                                & Trim(partie) & " 0-1" & vbCrLf & vbCrLf
            Else
                chaine = chaine & "[Result ""1-0""]" & vbCrLf & vbCrLf _
                                & Trim(partie) & " 1-0" & vbCrLf & vbCrLf
            End If
        End If

        Return chaine
    End Function

End Module

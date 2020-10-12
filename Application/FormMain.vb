Public Class frmMain

    Structure TPoint
        Dim x, y, z, w As Double
    End Structure

    Structure TLine
        Dim p1, p2 As Integer
        Dim acc As boolean
    End Structure

    Structure Cube
        Dim center() As Double
        Dim length As Double
        Dim color As System.Drawing.Color
    End Structure

    Dim Bmp As Bitmap
    Dim VRP(2), VPN(2), VUP(2), COP(2), Wmin(1), Wmax(1), FP, BP As Double
    Dim N(2), v(2), u(2), DOP(2), CW(2) As Double
    Dim Wt(3, 3), Vt(3, 3), St(3, 3) As Double

    Dim Cubes As New List(Of Cube)
    Dim Velocity As Decimal

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Bmp = New Bitmap(PictureBox1.Image)
        PictureBox1.Image = Bmp

        VRP = {0, 0, 0}
        VPN = {0, 0, 1}
        VUP = {0, 1, 0}
        COP = {0, 0, 4}
        Wmin = {-2, -2}
        Wmax = {2, 2}
        FP = 2
        BP = -10

        N = Normalize(VPN)

        v = Normalize(FindVector(MultiplyVectorByScalar(DotProduct(Normalize(VUP), N), N), Normalize(VUP)))

        u = CrossProduct(v, N)

        CW = {(Wmin(0) + Wmax(0)) / 2, (Wmin(1) + Wmax(1)) / 2, 0}
        DOP = {CW(0) - COP(0), CW(1) - COP(1), CW(2) - COP(2)}

        Velocity = 0.1

        lblFP.Text = "FP : " + FP.ToString
        lblBP.Text = "BP : " + BP.ToString
        lblMS.Text = "MS : " + Velocity.ToString

        SetRowMatrix(Wt, 0, 1, 0, 0, 0)
        SetRowMatrix(Wt, 1, 0, 1, 0, 0)
        SetRowMatrix(Wt, 2, 0, 0, 1, 0)
        SetRowMatrix(Wt, 3, 0, 0, 0, 1)

        PerspectiveProjection()

        SetRowMatrix(St, 0, 50, 0, 0, 0)
        SetRowMatrix(St, 1, 0, -50, 0, 0)
        SetRowMatrix(St, 2, 0, 0, 0, 0)
        SetRowMatrix(St, 3, 200, 200, 0, 1)

        Cubes.Add(CreateNewCube(-2, 0, 2, 1.5, Color.Red))
        Cubes.Add(CreateNewCube(0, 0, 2, 1.5, Color.Green))
        Cubes.Add(CreateNewCube(2, 0, 2, 1.5, Color.DarkBlue))
        Cubes.Add(CreateNewCube(-2, 0, 0, 1.5, Color.Pink))
        Cubes.Add(CreateNewCube(0, 0, 0, 1.5, Color.DarkOrange))
        Cubes.Add(CreateNewCube(2, 0, 0, 1.5, Color.DarkGray))
        Cubes.Add(CreateNewCube(-2, 0, -2, 1.5, Color.DeepPink))
        Cubes.Add(CreateNewCube(0, 0, -2, 1.5, Color.Black))
        Cubes.Add(CreateNewCube(2, 0, -2, 1.5, Color.DarkGreen))


        For i = Cubes.Count - 1 To 0 Step -1
            DrawCube(Cubes(i))
        Next
    End Sub

    Function CreateNewCube(ByVal centerX As Double, centerY As Double, centerZ As Double, length As Double, color As System.Drawing.Color) As Cube
        Dim cube As New Cube
        cube.center = {centerX, centerY, centerZ}
        cube.length = length
        cube.color = color

        Return cube
    End Function

    Function GetVectorLength(ByVal vector As Double()) As Double
        Dim length As Double = 0

        For Each value In vector
            length = length + Math.Pow(value, 2)
        Next

        Return Math.Sqrt(length)
    End Function

    Function Normalize(ByVal vector As Double()) As Double()
        Dim res() As Double = vector
        Dim length As Double = GetVectorLength(vector)

        '1 x 3 vector
        For i = 0 To 2 'vector.Length - 1
            res(i) = res(i) / length
        Next

        Return res
    End Function

    'Calculate vector of 2 points
    Function FindVector(ByVal p1 As Double(), p2 As Double()) As Double()
        Dim res(p1.Length - 1) As Double

        For i = 0 To res.Length - 1
            res(i) = p2(i) - p1(i)
        Next

        Return res
    End Function

    Function FindVector(ByVal p1 As TPoint, p2 As TPoint) As Double()
        Dim res(2) As Double

        res(0) = p2.x - p1.x
        res(1) = p2.y - p1.y
        res(2) = p2.z - p1.z

        Return res
    End Function

    Function MultiplyVectorByScalar(ByVal scalar As Double, vector As Double()) As Double()
        Dim res(vector.Length - 1) As Double

        For i = 0 To res.Length - 1
            res(i) = vector(i) * scalar
        Next

        Return res
    End Function

    Function DotProduct(ByVal v1 As Double(), v2 As Double()) As Double
        Dim res As Double = 0

        '1 x 3 matrix
        For i = 0 To 2 'v1.Length - 1
            res = res + v1(i) * v2(i)
        Next

        Return res
    End Function

    Function CrossProduct(ByVal v1 As Double(), v2 As Double()) As Double()
        Dim res(v1.Length - 1) As Double

        Dim next1, next2 As Double
        '1 x 3 matrix
        For i = 0 To 2 'v1.Length - 1
            next1 = i + 1
            next2 = i + 2
            If next1 = 2 Then 'v.Length - 1
                next2 = 0
            ElseIf next1 = 3 Then 'v1.Length
                next1 = 0
                next2 = 1
            End If
            res(i) = v1(next1) * v2(next2) - v2(next1) * v1(next2)
        Next

        Return res
    End Function

    Public Sub SetPoint(ByRef point As TPoint, ByVal x As Double, y As Double, z As Double)
        point.x = x
        point.y = y
        point.z = z
        point.w = 1
    End Sub

    Public Sub SetLine(ByRef line As TLine, ByVal p1 As Integer, p2 As Integer)
        line.p1 = p1
        line.p2 = p2
        line.acc = False
    End Sub

    Public Sub SetRowMatrix(ByRef M(,) As Double, ByVal row As Integer, a As Double, b As Double, c As Double, d As Double)
        M(row, 0) = a
        M(row, 1) = b
        M(row, 2) = c
        M(row, 3) = d
    End Sub

    Function MultiplyMatrix(ByVal p As TPoint, m(,) As Double) As TPoint
        Dim res As TPoint
        Dim w As Double

        w = (p.x * m(0, 3) + p.y * m(1, 3) + p.z * m(2, 3) + p.w * m(3, 3))

        res.x = (p.x * m(0, 0) + p.y * m(1, 0) + p.z * m(2, 0) + p.w * m(3, 0)) / w
        res.y = (p.x * m(0, 1) + p.y * m(1, 1) + p.z * m(2, 1) + p.w * m(3, 1)) / w
        res.z = (p.x * m(0, 2) + p.y * m(1, 2) + p.z * m(2, 2) + p.w * m(3, 2)) / w
        res.w = 1

        Return res
    End Function

    Function MultiplyMatrix(ByVal m1(,) As Double, m2(,) As Double) As Double(,)
        Dim row, col As Double
        row = m1.GetLength(0) - 1
        col = m2.GetLength(1) - 1

        Dim res(row, col) As Double

        '4 x 4 matrix
        For i = 0 To 3 'row
            For j = 0 To 3 'col
                res(i, j) = 0
                For k = 0 To 3 'row or col
                    res(i, j) = res(i, j) + m1(i, k) * m2(k, j)
                Next
            Next
        Next

        Return res
    End Function

    Dim T1T2(3, 3), T3(3, 3), T4(3, 3), T5(3, 3), T6(3, 3), T7(3, 3), T8(3, 3), T9(3, 3) As Double

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles lblFP.Click, lblMS.Click, lblBP.Click

    End Sub

    Dim Pr1(3, 3), Pr2(3, 3) As Double


    Public Sub PerspectiveProjection()


        SetRowMatrix(T1T2, 0, u(0), v(0), N(0), 0)
        SetRowMatrix(T1T2, 1, u(1), v(1), N(1), 0)
        SetRowMatrix(T1T2, 2, u(2), v(2), N(2), 0)
        SetRowMatrix(T1T2, 3, -1 * DotProduct(VRP, u), -1 * DotProduct(VRP, v), -1 * DotProduct(VRP, N), 1)

        SetRowMatrix(T3, 0, 1, 0, 0, 0)
        SetRowMatrix(T3, 1, 0, 1, 0, 0)
        SetRowMatrix(T3, 2, 0, 0, 1, 0)
        SetRowMatrix(T3, 3, -1 * COP(0), -1 * COP(1), -1 * COP(2), 1)

        Dim Shx, Shy As Double
        Shx = -1 * (DOP(0) / DOP(2))
        Shy = -1 * (DOP(1) / DOP(2))

        SetRowMatrix(T4, 0, 1, 0, 0, 0)
        SetRowMatrix(T4, 1, 0, 1, 0, 0)
        SetRowMatrix(T4, 2, Shx, Shy, 1, 0)
        SetRowMatrix(T4, 3, 0, 0, 0, 1)

        Dim w, h, BP4 As Double
        w = (COP(2) - BP) * (Wmax(0) - Wmin(0)) / (2 * COP(2))
        h = (COP(2) - BP) * (Wmax(1) - Wmin(1)) / (2 * COP(2))
        BP4 = BP - COP(2)

        SetRowMatrix(T5, 0, 1 / w, 0, 0, 0)
        SetRowMatrix(T5, 1, 0, 1 / h, 0, 0)
        SetRowMatrix(T5, 2, 0, 0, -1 / BP4, 0)
        SetRowMatrix(T5, 3, 0, 0, 0, 1)

        Dim VP5 As Double = COP(2) / (BP - COP(2))

        SetRowMatrix(T7, 0, 1, 0, 0, 0)
        SetRowMatrix(T7, 1, 0, 1, 0, 0)
        SetRowMatrix(T7, 2, 0, 0, 1, 0)
        SetRowMatrix(T7, 3, 0, 0, -1 * VP5, 1)

        Dim Vmax7 As Double = COP(2) / (COP(2) - BP)

        SetRowMatrix(T8, 0, 1 / Vmax7, 0, 0, 0)
        SetRowMatrix(T8, 1, 0, 1 / Vmax7, 0, 0)
        SetRowMatrix(T8, 2, 0, 0, 1, 0)
        SetRowMatrix(T8, 3, 0, 0, 0, 1)

        Dim COPz8 As Double = COP(2) / (COP(2) - BP) 'Vmax7

        SetRowMatrix(T9, 0, 1, 0, 0, 0)
        SetRowMatrix(T9, 1, 0, 1, 0, 0)
        SetRowMatrix(T9, 2, 0, 0, 1, -1 / COPz8)
        SetRowMatrix(T9, 3, 0, 0, 0, 1)



        SetRowMatrix(Pr2, 0, 1, 0, 0, 0)
        SetRowMatrix(Pr2, 1, 0, 1, 0, 0)
        SetRowMatrix(Pr2, 2, 0, 0, 0, 0)
        SetRowMatrix(Pr2, 3, 0, 0, 0, 1)

        Pr1 = MultiplyMatrix(T1T2, T3)
        Pr1 = MultiplyMatrix(Pr1, T4)
        Pr1 = MultiplyMatrix(Pr1, T5)
         Vt = Pr1
    End Sub

    Public Sub ClipLine(ByRef points() As TPoint, ByRef edge() As TLine)
        'N is an array of normal line of each surface
        'P is an array of point on the surface
        'Order : front, back, right, left, top, bottom
        Dim N(5)() As Double
        Dim P(5) As TPoint

        N(0) = {0, 0, -1}
        N(1) = {0, 0, 1}
        N(2) = {-1, 0, -1}
        N(3) = {1, 0, -1}
        N(4) = {0, -1, -1}
        N(5) = {0, 1, -1}

        Dim F5 As Double
        F5 = (FP - COP(2)) / (COP(2) - BP)

        SetPoint(P(0), 0, 0, F5)
        SetPoint(P(1), 0, 0, -1)
        SetPoint(P(2), 0, 0, 0)
        SetPoint(P(3), 0, 0, 0)
        SetPoint(P(4), 0, 0, 0)
        SetPoint(P(5), 0, 0, 0)

        For j = 0 To 11


            Dim p1, p2 As TPoint
            p1 = points(edge(j).p1)
            p2 = points(edge(j).p2)

            Dim status(5) As String

            For i = 0 To 5
                Dim D1, D2 As Double
                D1 = DotProduct(FindVector(P(i), p1), N(i))
                D2 = DotProduct(FindVector(P(i), p2), N(i))

                'Check line status
                If D1 >= 0 And D2 >= 0 Then     'Trivially accepted
                    status(i) = "TAccepted"
                ElseIf D1 < 0 And D2 < 0 Then   'Trivially rejected
                    status(i) = "TRejected"
                Else                            'Partially accepted
                    status(i) = "PAccepted"
                End If
            Next

            If Not status.Contains("TRejected") Then    'Line is not rejected
                Dim maxE, minL As Double
                maxE = 0
                minL = 1

                For i = 0 To 5
                    If status(i).Equals("PAccepted") Then
                        Dim t, D As Double
                        D = DotProduct(FindVector(p1, p2), N(i))
                        t = DotProduct(FindVector(p1, P(i)), N(i)) / D

                        't is entering and is bigger than the current maximum t entering
                        If D > 0 And t > maxE Then
                            maxE = t
                        End If

                        't is leaving and is smaller than the current minimum t leaving
                        If D < 0 And t < minL Then
                            minL = t
                        End If
                    End If
                Next

                If maxE < minL Then
                    Dim temp = MultiplyVectorByScalar(minL, FindVector(p1, p2))
                    p2.x = p1.x + temp(0)
                    p2.y = p1.y + temp(1)
                    p2.z = p1.z + temp(2)

                    temp = MultiplyVectorByScalar(maxE, FindVector(p1, p2))
                    p1.x = p1.x + temp(0)
                    p1.y = p1.y + temp(1)
                    p1.z = p1.z + temp(2)

                    edge(j).acc = True
                End If
            End If
        Next
    End Sub

    Public Sub PSet(ByVal x As Integer, y As Integer, color As System.Drawing.Color)
        If x > 0 And y > 0 And x < Bmp.Width And y < Bmp.Height Then
            Bmp.SetPixel(x, y, color)
        End If
    End Sub

    Public Sub DrawCube(ByVal cube As Cube)
        Dim V(7), VW(7), VV(7), VS(7) As TPoint
        Dim Edges(11) As TLine
        Dim x, y, z, len As Double

        x = cube.center(0)
        y = cube.center(1)
        z = cube.center(2)
        len = cube.length / 2

        SetPoint(V(0), x - len, y - len, z + len)
        SetPoint(V(1), x + len, y - len, z + len)
        SetPoint(V(2), x + len, y + len, z + len)
        SetPoint(V(3), x - len, y + len, z + len)
        SetPoint(V(4), x - len, y - len, z - len)
        SetPoint(V(5), x + len, y - len, z - len)
        SetPoint(V(6), x + len, y + len, z - len)
        SetPoint(V(7), x - len, y + len, z - len)

        SetLine(Edges(0), 0, 1)
        SetLine(Edges(1), 1, 2)
        SetLine(Edges(2), 2, 3)
        SetLine(Edges(3), 3, 0)
        SetLine(Edges(4), 4, 5)
        SetLine(Edges(5), 5, 6)
        SetLine(Edges(6), 6, 7)
        SetLine(Edges(7), 7, 4)
        SetLine(Edges(8), 0, 4)
        SetLine(Edges(9), 1, 5)
        SetLine(Edges(10), 2, 6)
        SetLine(Edges(11), 3, 7)

        For i = 0 To 7
            VW(i) = MultiplyMatrix(V(i), Wt)
            VV(i) = MultiplyMatrix(VW(i), Vt)

        Next

        ClipLine(VV, Edges)

        For i = 0 To 7
            VS(i) = MultiplyMatrix(VV(i), T7)
            VS(i) = MultiplyMatrix(VS(i), T8)
            VS(i) = MultiplyMatrix(VS(i), T9)
            VS(i) = MultiplyMatrix(VS(i), Pr2)
            VS(i) = MultiplyMatrix(VS(i), St)
            'ListView1.Items.Add(VS(i).x.ToString + " " + VS(i).y.ToString + " " + VS(i).z.ToString + " " + VS(i).w.ToString)
        Next



        Dim p1, p2 As TPoint

        For i = 0 To 11
            If Edges(i).acc Then
                p1 = VS(Edges(i).p1)
                p2 = VS(Edges(i).p2)

                DrawLine(p1.x, p1.y, p2.x, p2.y, cube.color)
            End If
        Next
    End Sub

    Public Sub DrawLine(ByVal x1 As Double, y1 As Double, x2 As Double, y2 As Double, color As System.Drawing.Color)
        Dim dx, dy As Integer
        dx = Math.Abs(x2 - x1)
        dy = Math.Abs(y2 - y1)

        Dim m, n As Double
        m = dy / dx
        n = dx / dy

        Dim x, y As Double
        x = Math.Round(x1)
        y = Math.Round(y1)
        If x1 = x2 And y1 = y2 Then ' Dot
            PSet(x, y, color)
        ElseIf y1 = y2 Then 'Horizontal
            If x1 < x2 Then 'Right
                While x <= x2
                    PSet(x, y, color)
                    x = x + 1
                End While
            ElseIf x1 > x2 Then 'Left
                While x >= x2
                    PSet(x, y, color)
                    x = x - 1
                End While
            End If
        ElseIf x1 = x2 Then 'Vertical
            If y1 <= y2 Then 'Up
                While y < y2
                    PSet(x, y, color)
                    y = y + 1
                End While
            ElseIf y1 > y2 Then 'Down
                While y >= y2
                    PSet(x, y, color)
                    y = y - 1
                End While
            End If
        ElseIf Math.Abs(dx) = Math.Abs(dy) Then 'Oblique
            If x1 < x2 Then 'Left to Right
                If y1 < y2 Then 'Up
                    While x <= x2
                        PSet(x, y, color)
                        x = x + 1
                        y = y + 1
                    End While
                ElseIf y1 > y2 Then 'Down
                    While x <= x2
                        PSet(x, y, color)
                        x = x + 1
                        y = y - 1
                    End While
                End If
            ElseIf x1 > x2 Then 'Right to Left
                If y1 < y2 Then 'Up
                    While x >= x2
                        PSet(x, y, color)
                        x = x - 1
                        y = y + 1
                    End While
                ElseIf y1 > y2 Then 'Down
                    While x >= x2
                        PSet(x, y, color)
                        x = x - 1
                        y = y - 1
                    End While
                End If
            End If
        ElseIf dx > dy Then 'Not steep
            If x1 < x2 Then 'Left to Right
                If y1 < y2 Then 'Up
                    While x <= x2
                        PSet(x, Math.Round(y), color)
                        x = x + 1
                        y = y + m
                    End While
                ElseIf y1 > y2 Then 'Down
                    While x <= x2
                        PSet(x, Math.Round(y), color)
                        x = x + 1
                        y = y - m
                    End While
                End If
            ElseIf x1 > x2 Then 'Right to Left
                If y1 < y2 Then 'Up
                    While x >= x2
                        PSet(x, Math.Round(y), color)
                        x = x - 1
                        y = y + m
                    End While
                ElseIf y1 > y2 Then 'Down
                    While x >= x2
                        PSet(x, Math.Round(y), color)
                        x = x - 1
                        y = y - m
                    End While
                End If
            End If
        ElseIf dy > dx Then 'Steep
            If y1 < y2 Then 'Up
                If x1 < x2 Then 'Left to Right
                    While y <= y2
                        PSet(Math.Round(x), y, color)
                        x = x + n
                        y = y + 1
                    End While
                ElseIf x1 > x2 Then 'Right to Left
                    While y <= y2
                        PSet(Math.Round(x), y, color)
                        x = x - n
                        y = y + 1
                    End While
                End If
            ElseIf y1 > y2 Then 'Down
                If x1 < x2 Then 'Left to Right
                    While y >= y2
                        PSet(Math.Round(x), y, color)
                        x = x + n
                        y = y - 1
                    End While
                ElseIf x1 > x2 Then 'Right to Left
                    While y >= y2
                        PSet(Math.Round(x), y, color)
                        x = x - n
                        y = y - 1
                    End While
                End If
            End If
        End If
    End Sub

    Function Sin(ByVal deg As Double) As Double
        Return Math.Sin(deg * Math.PI / 180)
    End Function

    Function Cos(ByVal deg As Double) As Double
        Return Math.Cos(deg * Math.PI / 180)
    End Function

    Private Sub tmrFrame_Tick(sender As Object, e As EventArgs) Handles tmrFrame.Tick
        ClearScreen()
        MoveCamera()
    End Sub

    Dim Vx, Vz As Decimal
    Dim Forward, Backward, Right, Left As Boolean

    Public Sub MoveCamera()
        VRP(0) = VRP(0) + Vx
        VRP(2) = VRP(2) + Vz

        PerspectiveProjection()

        For i = Cubes.Count - 1 To 0 Step -1
            DrawCube(Cubes(i))
        Next
        PictureBox1.Refresh()
    End Sub

    Public Sub ClearScreen()
        For i = 0 To Bmp.Height - 1
            For j = 0 To Bmp.Width - 1
                PSet(i, j, Color.White)
            Next
        Next

        PictureBox1.Refresh()
    End Sub

    Private Sub frmMain_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Up Then
            Vz = -Velocity
            Forward = True
            tmrFrame.Enabled = True
        End If
        If e.KeyCode = Keys.Down Then
            Vz = Velocity
            Backward = True
            tmrFrame.Enabled = True
        End If
        If e.KeyCode = Keys.Right Then
            Vx = Velocity
            Right = True
            tmrFrame.Enabled = True
        End If
        If e.KeyCode = Keys.Left Then
            Vx = -Velocity
            Left = True
            tmrFrame.Enabled = True
        End If
    End Sub

    Private Sub frmMain_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.Up Then
            Forward = False
            If Backward Then
                Vz = Velocity
            Else
                Vz = 0
                If Not (Right Or Left) Then
                    Vz = 0
                    tmrFrame.Enabled = False
                End If
            End If
        End If
        If e.KeyCode = Keys.Down Then
            Backward = False
            If Forward Then
                Vz = -1 * Velocity
            Else
                Vz = 0
                If Not (Right Or Left) Then
                    Vz = 0
                    tmrFrame.Enabled = False
                End If
            End If
        End If
        If e.KeyCode = Keys.Right Then
            Right = False
            If Left Then
                Vx = -1 * Velocity
            Else
                Vx = 0
                If Not (Forward Or Backward) Then
                    tmrFrame.Enabled = False
                End If
            End If
        End If
        If e.KeyCode = Keys.Left Then
            Left = False
            If Right Then
                Vx = Velocity
            Else
                Vx = 0
                If Not (Forward Or Backward) Then
                    tmrFrame.Enabled = False
                End If
            End If
        End If
    End Sub

    Private Sub frmMain_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        'FP : W S
        'BP : E D
        'MS : Q A
        If e.KeyChar = Convert.ToChar(115) Or e.KeyChar = Convert.ToChar(83) Then 'S key
            FP = FP - 1
            lblFP.Text = "FP : " + FP.ToString
        End If
        If e.KeyChar = Convert.ToChar(119) Or e.KeyChar = Convert.ToChar(87) Then 'W key
            FP = FP + 1
            lblFP.Text = "FP : " + FP.ToString
        End If
        If e.KeyChar = Convert.ToChar(101) Or e.KeyChar = Convert.ToChar(69) Then 'E key
            BP = BP - 1
            lblBP.Text = "BP : " + BP.ToString
        End If
        If e.KeyChar = Convert.ToChar(100) Or e.KeyChar = Convert.ToChar(68) Then 'D key
            BP = BP + 1
            lblBP.Text = "BP : " + BP.ToString
        End If
        If e.KeyChar = Convert.ToChar(97) Or e.KeyChar = Convert.ToChar(65) Then 'A key
            If Not Velocity <= 0.1 Then
                Velocity = Velocity - 0.1
                If Right Then
                    Vx = Vx - 0.1
                ElseIf Left Then
                    Vx = Vx + 0.1
                End If
                If Forward Then
                    Vz = Vz + 0.1
                ElseIf Backward Then
                    Vz = Vz - 0.1
                End If
                lblMS.Text = "MS : " + Velocity.ToString
            End If
        End If
        If e.KeyChar = Convert.ToChar(113) Or e.KeyChar = Convert.ToChar(81) Then 'Q key
            Velocity = Velocity + 0.1
            If Right Then
                Vx = Vx + 0.1
            ElseIf Left Then
                Vx = Vx - 0.1
            End If
            If Forward Then
                Vz = Vz - 0.1
            ElseIf Backward Then
                Vz = Vz + 0.1
            End If
            lblMS.Text = "MS : " + Velocity.ToString
        End If
    End Sub
End Class


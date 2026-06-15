Namespace VisualBasicProject
    Public Module Module1
        <Obsolete("Use NewMethod", False)>
        Public Sub OldMethod()
        End Sub

        Public Sub TriggerWarning()
            OldMethod()
        End Sub
    End Module
End Namespace

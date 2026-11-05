using UnityEngine;

// Gunakan HANYA di scene UjianKPKFPB.
// Wajib ada QuizKPKFPBManager.cs pada proyek (pewarisan).
// Bank 30 pola & sesi 10 soal ditangani kelas induk dengan ModeUjian = true.
public class UjianKPKFPBManager : QuizKPKFPBManager
{
    protected override bool ModeUjian { get { return true; } }
}

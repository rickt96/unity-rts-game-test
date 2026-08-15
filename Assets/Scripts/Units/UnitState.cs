namespace Tactical.Units
{
    // Stati centralizzati del soldato (squadra e nemici). Ogni stato
    // corrisponde 1:1 a un trigger di animazione da collegare in una fase
    // successiva (corsa=Moving, mira=Aiming, sparo=Firing, entrata in
    // copertura=EnteringCover, pressione da sbarramento=Suppressed,
    // ferimento=Wounded, morte=Dead).
    public enum UnitState
    {
        Idle,
        Moving,
        EnteringCover,
        InCover,
        Aiming,
        Firing,
        Suppressed,
        SurvivalReflex,
        Wounded,
        Dead
    }
}

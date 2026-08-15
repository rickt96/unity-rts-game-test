# Brief di progetto — Tactical Squad Shooter (ispirato a Full Spectrum Warrior)

Sto sviluppando una demo di un videogioco strategico tattico in tempo reale, ambientato in scenario bellico. Prima di scrivere codice, leggi questo brief per avere il contesto completo del design. Se qualcosa non è chiaro o vedi conflitti tra le regole, chiedimelo prima di procedere.

## Panoramica

- Genere: strategico tattico a comando indiretto, ispirato a *Full Spectrum Warrior*.
- Il giocatore **non controlla direttamente** un soldato: impartisce ordini alla squadra.
- Camera in **terza persona**, agganciata alla squadra/al soldato leader — il giocatore vede quello che vede la squadra, non una visuale dall'alto.
- Mappe 3D di scenario bellico, dimensione media, **non open-world** (livelli delimitati e progettati a mano).
- Engine: Unity (C#).

## Struttura della squadra

- La squadra è divisa in ruoli fissi: TL (Team Leader), AR (Automatic Rifleman), G (Grenadier), R (Rifleman).
- Il giocatore seleziona un ruolo/membro tramite un selettore (radiale o a icone) e impartisce ordini contestuali al bersaglio selezionato.
- Ordini base previsti nella demo: Move To, Take Cover, Suppress/Attack sul bersaglio puntato.
- Possono esserci più team sul campo (es. Alpha, Bravo, Charlie), identificati con etichette world-space fluttuanti sopra il gruppo.

## Sistema di combattimento e coperture

Questo è il cuore delle meccaniche, va progettato con attenzione:

1. **Stati di esposizione**: ogni unità (sia della squadra che nemica) può trovarsi in due stati: **all'aperto** o **in copertura**. Questa regola è simmetrica: si applica sia alla squadra del giocatore sia ai nemici.

2. **Efficacia della copertura in base all'angolo**: la copertura non è un valore fisso booleano ma dipende dalla posizione relativa tra chi spara e chi si copre.
   - Se il nemico che spara ha un angolo di tiro libero rispetto all'unità in copertura (perché è posizionato più in alto o lateralmente rispetto all'ostacolo), la copertura perde efficacia.
   - Più il tiratore è disallineato rispetto alla normale della copertura (in altezza o lateralmente), meno protezione offre l'ostacolo.
   - Serve quindi un calcolo geometrico (es. angolo tra la linea di tiro e la copertura, eventualmente combinato con raycast/line-of-sight) che determini un fattore di riduzione del danno o della probabilità di colpire, non solo un flag "coperto/scoperto".

3. **Volume di fuoco**: un'unità può essere sottoposta a intensità di fuoco diverse in base a:
   - il numero di nemici che le stanno sparando contemporaneamente;
   - il tipo di arma di ciascun nemico (es. fucile automatico vs mitragliatrice vs granate — cadenza di fuoco, danno per colpo, effetto di soppressione).
   - Il volume di fuoco complessivo dovrebbe influenzare non solo il danno ricevuto ma anche lo stato "soppresso" dell'unità (capacità di muoversi/mirare mentre sotto fuoco pesante).

## Sistema di animazione (fase successiva alla demo)

Non è richiesto nella prima vertical slice, ma l'architettura del combattimento e del controller dei soldati deve essere pensata fin da ora per accogliere in un secondo momento un set di animazioni individuali, preferibilmente acquisite tramite asset esterni (es. Mixamo o asset store) piuttosto che create da zero. Le animazioni previste per ogni soldato (squadra e nemici) sono:

- Corsa
- Mira
- Sparo
- Entrata in copertura
- Pressione da sbarramento (reazione al fuoco di soppressione, mentre "sotto tiro")
- Ferimento
- Morte

Questo implica che lo stato di ogni unità (all'aperto/in copertura, sotto fuoco/soppresso, ferito, morto) deve essere modellato in modo esplicito e centralizzato (es. una state machine) fin dalla vertical slice, così che in futuro sia sufficiente collegare i trigger di animazione agli stati già esistenti, senza dover riscrivere la logica di combattimento.

## Sistema di danno e morte

Data la natura strategica del gioco, punto a un sistema realistico basato su **esposizione al fuoco nel tempo** piuttosto che su barre vita/hit point tradizionali:

- Un soldato (squadra o nemico) muore quando rimane esposto al fuoco nemico per un tempo continuativo superiore a una soglia X.
- Questa soglia X **non è fissa**, ma è influenzata da:
  - il numero di nemici che stanno puntando/sparando contemporaneamente sull'unità (collegato al concetto di volume di fuoco descritto sopra);
  - la tipologia di armi dei nemici che stanno sparando (arma automatica, di precisione, esplosivi, ecc. — ognuna con un peso diverso sul tempo di esposizione tollerato);
  - il livello di difficoltà scelto dal giocatore.
- È quindi un sistema "one-shot" nel senso che non c'è un pool di hit point da erodere colpo per colpo: la morte è un evento che scatta al superamento della soglia di esposizione, non una somma di danni discreti.
- Questo sistema deve integrarsi con l'efficacia della copertura ad angolo variabile descritta sopra: la copertura (ridotta in base all'angolo del tiratore) dovrebbe agire riducendo l'accumulo del "tempo di esposizione effettivo", non solo bloccando il danno in modo binario.

Da stabilire insieme in fase di progettazione: come esporre questi parametri (soglia base, pesi per arma, moltiplicatore di difficoltà) in modo che siano facilmente bilanciabili senza toccare il codice — es. ScriptableObject di configurazione in Unity.

## UI di comando e posizionamento in copertura

- Il controllo della squadra avviene tramite un **indicatore ad anello** che segue il cursore/mirino sullo schermo.
- Quando l'anello passa sopra una copertura valida, si **aggancia** ad essa (snapping) e mostra un'**anteprima** di come la squadra si disporrà se il giocatore conferma l'ordine in quel punto (silhouette o marker per ogni soldato nella posizione che occuperebbe).
- Ogni copertura ha un **numero di slot** disponibili, cioè quante posizioni di soldati può ospitare fisicamente lungo il suo fronte.
- Se il numero di soldati ordinati supera gli slot disponibili sulla copertura:
  - i soldati che occupano uno slot si schierano normalmente (coperti secondo le regole di angolo già descritte);
  - i soldati in eccedenza **si accodano in stack dietro** ai primi, nella stessa posizione/area;
  - un soldato in stack è comunque **coperto frontalmente** (beneficia della stessa copertura fisica), ma è **maggiormente esposto su angoli alti o laterali** rispetto ai soldati che occupano uno slot proprio, perché non ha l'ostacolo fisico direttamente allineato a proteggerlo su quei lati.

Questo significa che il modello di "efficacia della copertura in base all'angolo" descritto sopra deve prevedere un parametro aggiuntivo per unità: se occupa uno **slot proprio** sulla copertura o è **in stack** dietro un altro soldato, dato che quest'ultimo caso riduce ulteriormente l'efficacia della copertura sugli angoli laterali/alti anche a parità di posizione del tiratore nemico.

## Autonomia del soldato (riflesso di sopravvivenza)

Il gioco è a comando indiretto e la componente strategica deve restare del giocatore, ma per credibilità tattica ogni soldato (squadra e nemici, regola simmetrica come per la copertura) ha un **riflesso di sopravvivenza locale**, non una vera autonomia decisionale:

- **Trigger di attivazione**: l'unità è esposta (copertura assente o azzerata dall'angolo del tiratore) e sottoposta a un volume di fuoco sopra una soglia definita, mantenuto per un breve periodo di grazia (per evitare che scatti per un singolo colpo isolato o sporadico).
- **Azione**: l'unità cerca autonomamente la copertura più efficace **entro un raggio limitato** attorno alla propria posizione attuale — riutilizzando la stessa funzione di scoring/valutazione copertura impiegata per l'anteprima dell'indicatore ad anello, applicata però a un'area ristretta invece che al punto scelto dal giocatore.
- **Limite**: è un riflesso locale e temporaneo. Non ripianifica la tattica, non attraversa la mappa, non rompe la formazione, non decide autonomamente di ritirarsi o avanzare.

Regole di precedenza e comportamento, da implementare così:

1. **Priorità sugli ordini**: il riflesso di sopravvivenza ha sempre priorità su un ordine esplicito come Hold Position — un'unità non resta ferma sotto fuoco pesante per pura disciplina.
2. **Raggio di ricerca**: parametrizzato (costante base, eventualmente scalabile con la difficoltà — unità meno esperte potrebbero muoversi meno o scegliere coperture subottimali).
3. **Comportamento post-minaccia**: quando il volume di fuoco scende sotto soglia, l'unità **non torna automaticamente** alla posizione originariamente ordinata; la nuova posizione raggiunta per riflesso diventa la baseline corrente, fino a un nuovo ordine esplicito del giocatore (evita comportamenti a "yo-yo" tra due punti).
4. **Ordini aggressivi**: un ordine di tipo Assault/Suppress **sopprime** il riflesso di sopravvivenza per la durata dell'ordine, per non rompere un'azione voluta esplicitamente dal giocatore (es. un soldato mandato all'assalto non deve tuffarsi dietro un muro a metà corsa).

## Scope della demo (vertical slice)

- 1 squadra giocabile (4 membri con i ruoli sopra).
- 1 mappa piccola con 4-5 punti di copertura preposizionati.
- 2-3 nemici (statici o con pattugliamento semplice a waypoint).
- I 3 ordini base (Move To, Take Cover, Suppress/Attack).
- Il sistema di copertura ad angolo variabile e il volume di fuoco descritti sopra, anche in versione semplificata ma funzionante.

## Cosa mi serve da te in questa fase

Non scrivere ancora codice. Prima:
1. Proponi un'architettura di sistemi (script principali, come rappresentare cover point con i relativi slot, come calcolare l'efficacia della copertura in base ad angolo e posizione in-slot/in-stack, come gestire il volume di fuoco e l'accumulo del tempo di esposizione in modo scalabile) coerente con quanto descritto sopra.
2. Proponi come modellare lo stato di ogni unità (state machine) in modo che sia già pronto ad accogliere in futuro i trigger di animazione (corsa, mira, sparo, entrata in copertura, pressione da sbarramento, ferimento, morte) senza refactoring, e che integri il riflesso di sopravvivenza come stato/transizione a sé stante con priorità sugli ordini normali.
3. Proponi come strutturare l'indicatore ad anello e la logica di snapping/anteprima sugli slot di copertura, includendo come gestire il caso di eccedenza di soldati (stacking), e come riutilizzare la stessa funzione di scoring per il riflesso di sopravvivenza.
4. Segnala eventuali ambiguità nelle regole di copertura/volume di fuoco/soglia di esposizione/slot/riflesso di sopravvivenza che vanno chiarite prima di implementare.
5. Suggerisci l'ordine di implementazione più sensato per validare rapidamente il "feel" del combattimento nella demo.

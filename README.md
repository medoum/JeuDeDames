# Jeu de dames

Jeu de dames international (10×10) en .NET 10 : moteur de règles en C# et interface Blazor WebAssembly.

## Structure

- `src/JeuDeDames.Core`: moteur de jeu, sans dépendance à l'interface (plateau, coups, règles, partie).
- `src/JeuDeDames.Web`: interface Blazor WebAssembly.
- `tests/JeuDeDames.Core.Tests`: tests xUnit des règles.

## Commandes

```bash
dotnet run --project src/JeuDeDames.Web   # http://localhost:5266
dotnet test
```

## Règles implémentées

- Les blancs commencent ; les pions avancent d'une case en diagonale.
- La prise est obligatoire, et il faut prendre le maximum de pièces (règle de la majorité).
- Les pions prennent vers l'avant et vers l'arrière.
- Les dames sont volantes : elles se déplacent et prennent à distance.
- Les pièces prises restent sur le plateau jusqu'à la fin de la rafle et ne peuvent pas être sautées deux fois.
- Un pion n'est promu dame que s'il termine son coup sur la dernière rangée.
- Un joueur qui ne peut plus jouer a perdu.

## Ordinateur

L'IA (`src/JeuDeDames.Core/Ai.cs`) utilise une recherche negamax avec élagage alpha-bêta et approfondissement itératif. Elle ne s'arrête pas au milieu d'une série de prises, et elle évalue le matériel (dame = 3 pions) ainsi que l'avancement et la centralisation des pions.

| Niveau    | Profondeur max | Temps max |
|-----------|----------------|-----------|
| Facile    | 2              | 0,3 s     |
| Moyen     | 4              | 0,8 s     |
| Difficile | 10             | 1,5 s     |

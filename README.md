# Prática em Laboratório UNITY (Steering Behaviors)

DESCRIÇÃO:

Originalmente haviam dois comportamentos para os agentes, interpretados por mim da seguinte forma:

-SEEK: Npc agressivo, ele persegue o jogador até alcançá-lo.

-EVADE: Npc cauteloso, ele mantém uma distância segura do jogador.

Nessa mesma linha, os comportamentos adicionados foram:

-FLEE: Npc medroso, é semelhante a EVADE, porém utiliza um booleano, para garantir que, uma vez que o jogador se aproximou, o agente continue fugindo eternamente.

-PURSUIT: Npc aliado, semelhante ao SEEK, ele persegue o jogador, no entanto, ao alcançá-lo ele se torna um objeto filho, acompanhando os movimentos do jogador.

-COHESION: Npc assustador, semelhante ao SEEK, neste comportamento, o Npc se move até o jogador, mas para a uma distância considerável, mantendo um comportamento de stalker.

-WALLAVOID: Este é um comportamento pensado para se mesclar aos outros, utiliza triggers para identificar objetos com a tag "wall", para identificar o pontos mais próximo de seus colliders, para então calcular a rota e se afastar das paredes.

 

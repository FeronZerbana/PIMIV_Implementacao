import 'package:flutter/material.dart';

class HomeTab extends StatelessWidget {
  const HomeTab({super.key});

  @override
  Widget build(BuildContext context) {
    Widget _buildBodyBack() =>
        Container(
          decoration: BoxDecoration(
              gradient: LinearGradient(
                  colors: [
                    Color.fromARGB(255, 75, 0, 130),
                    Color.fromARGB(255, 255, 255, 255)
                  ],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight
              )
          ),
        );
    return Stack(
      children: <Widget>[
        _buildBodyBack(),
        CustomScrollView(
          slivers: [
            SliverAppBar(
              floating: true,
              snap: true,
              backgroundColor: Colors.transparent,
              elevation: 0,

              // Deixa o botão de três riscos branco
              iconTheme: const IconThemeData(
                color: Colors.white,
              ),

              // Deixa o título Novidades branco
              flexibleSpace: const FlexibleSpaceBar(
                title: Text(
                  "Novidades",
                  style: TextStyle(
                    color: Colors.white,
                  ),
                ),
                centerTitle: true,
              ),
            ),
            SliverToBoxAdapter(
                child: GridView.count(
                  crossAxisCount: 2,
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  padding: const EdgeInsets.all(0),
                  crossAxisSpacing: 0,
                  mainAxisExtent: 290,
                  childAspectRatio: 1.02,
                  children: [
                    imagens('assets/imagens/bateria.jpg'),
                    imagens('assets/imagens/guitarra.jpg'),
                    imagens('assets/imagens/instrumentos.jpg'),
                    imagens('assets/imagens/violino.jpg'),
                    imagens('assets/imagens/baquetas.jpg'),
                    imagens('assets/imagens/amplificador.jpg'),
                  ],
                )
            )
          ],
        )
      ],
    );
  }
  Widget imagens(String caminho) {
    return Container(
      width: double.infinity,
      height: double.infinity,
      clipBehavior: Clip.antiAlias,
      decoration: const BoxDecoration(),
      child: Image.asset(
        caminho,
        width: double.infinity,
        height: double.infinity,
        fit: BoxFit.cover,
        alignment: Alignment.center,
      ),
    );
  }
}


import 'package:apppim/models/user_model.dart';
import 'package:apppim/screens/login_screen.dart';
import 'package:apppim/tiles/drawer_tile.dart';
import 'package:apppim/widgets/server_settings_dialog.dart';
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';

class CustomDrawer extends StatelessWidget {
  final PageController pageController;

  const CustomDrawer(this.pageController, {super.key});

  Widget _buildDrawerBack() {
    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: [
            Color.fromARGB(255, 203, 236, 241),
            Colors.white,
          ],
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: Stack(
        children: [
          _buildDrawerBack(),
          ListView(
            padding: const EdgeInsets.only(
              left: 24.0,
              top: 16.0,
              right: 16.0,
            ),
            children: [
              Container(
                margin: const EdgeInsets.only(bottom: 8.0),
                padding: const EdgeInsets.fromLTRB(0, 16, 16, 8),
                height: 170,
                child: Stack(
                  children: [
                    const Positioned(
                      top: 8,
                      left: 0,
                      child: Text(
                        'Shop\nNow',
                        style: TextStyle(
                          fontSize: 34,
                          fontWeight: FontWeight.bold,
                          letterSpacing: 1.2,
                        ),
                      ),
                    ),
                    Positioned(
                      left: 0,
                      bottom: 0,
                      child: ScopedModelDescendant<UserModel>(
                        builder: (context, child, model) {
                          final bool loggedIn = model.isLoggedIn();

                          final String userName = loggedIn
                              ? (model.userdata['name'] ?? '').toString()
                              : '';

                          return Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Ola, $userName',
                                style: const TextStyle(
                                  fontSize: 18,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              GestureDetector(
                                child: Text(
                                  loggedIn
                                      ? 'Sair'
                                      : 'Entre ou cadastre-se >',
                                  style: TextStyle(
                                    color: Theme.of(context).primaryColor,
                                    fontSize: 16,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                onTap: () async {
                                  if (!loggedIn) {
                                    Navigator.of(context).pop();

                                    await Navigator.of(context).push(
                                      MaterialPageRoute(
                                        builder: (_) => const LoginScreen(),
                                      ),
                                    );
                                  } else {
                                    model.signOut();

                                    Navigator.of(context).pop();

                                    ScaffoldMessenger.of(context).showSnackBar(
                                      const SnackBar(
                                        content: Text(
                                          'Voce saiu da sua conta.',
                                        ),
                                      ),
                                    );
                                  }
                                },
                              ),
                            ],
                          );
                        },
                      ),
                    ),
                  ],
                ),
              ),

              const Divider(),

              DrawerTile(
                Icons.home,
                'Inicio',
                pageController,
                0,
              ),

              DrawerTile(
                Icons.list,
                'Produtos',
                pageController,
                1,
              ),

              DrawerTile(
                Icons.playlist_add_check,
                'Meus Pedidos',
                pageController,
                2,
              ),

              const Divider(),

              ListTile(
                leading: Icon(
                  Icons.dns,
                  color: Theme.of(context).primaryColor,
                ),
                title: const Text(
                  'Configuracoes de Servidor / API',
                  style: TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                ),
                onTap: () {
                  Navigator.of(context).pop();
                  ServerSettingsDialog.show(context);
                },
              ),
            ],
          ),
        ],
      ),
    );
  }
}

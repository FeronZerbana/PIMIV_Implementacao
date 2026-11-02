
import 'package:flutter/material.dart';

class DrawerTile extends StatelessWidget {
  final IconData icon;
  final String text;
  final PageController controller;
  final int page;

  const DrawerTile(
      this.icon,
      this.text,
      this.controller,
      this.page, {
        super.key,
      });

  @override
  Widget build(BuildContext context) {
    final int currentPage =
    (controller.page ?? controller.initialPage.toDouble()).round();

    final bool selected = currentPage == page;

    final Color itemColor = selected
        ? Theme.of(context).primaryColor
        : Colors.grey[700]!;

    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () {
          Navigator.of(context).pop();

          if (controller.hasClients) {
            controller.animateToPage(
              page,
              duration: const Duration(milliseconds: 300),
              curve: Curves.ease,
            );
          }
        },
        child: SizedBox(
          height: 60.0,
          child: Row(
            children: [
              Icon(
                icon,
                size: 32.0,
                color: itemColor,
              ),
              const SizedBox(width: 32.0),
              Text(
                text,
                style: TextStyle(
                  fontSize: 16.0,
                  color: itemColor,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

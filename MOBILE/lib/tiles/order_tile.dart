
import 'package:flutter/material.dart';

class OrderTile extends StatelessWidget {
  final String orderId;
  final DateTime orderDate;
  final int itemCount;
  final double total;
  final int currentStep;

  const OrderTile(
      this.orderId, {
        required this.orderDate,
        required this.itemCount,
        required this.total,
        this.currentStep = 1,
        super.key,
      });

  Widget _buildStep(int number, String label) {
    final bool completed = currentStep >= number;

    return Expanded(
      child: Column(
        children: [
          CircleAvatar(
            radius: 18,
            backgroundColor:
            completed ? Colors.green : Colors.grey.shade300,
            child: Text(
              '$number',
              style: TextStyle(
                color: completed ? Colors.white : Colors.grey.shade700,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(height: 8),
          Text(
            label,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 12,
              color: completed ? Colors.green : Colors.grey,
              fontWeight:
              completed ? FontWeight.bold : FontWeight.normal,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildProgressLine(bool completed) {
    return Expanded(
      child: Container(
        height: 3,
        margin: const EdgeInsets.only(bottom: 24),
        color: completed ? Colors.green : Colors.grey.shade300,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final dateFormat = MaterialLocalizations.of(context);

    final currency = 'R\$ ${total.toStringAsFixed(2).replaceAll('.', ',')}';

    final int step = currentStep.clamp(1, 3);

    return Card(
      margin: const EdgeInsets.symmetric(
        horizontal: 10,
        vertical: 8,
      ),
      elevation: 3,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Pedido #$orderId',
              style: const TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Data: ${dateFormat.formatMediumDate(orderDate)}',
            ),
            Text('$itemCount produto(s)'),
            Text(
              'Total: $currency',
              style: const TextStyle(
                fontWeight: FontWeight.bold,
                fontSize: 15,
              ),
            ),
            const SizedBox(height: 20),
            const Divider(),
            const SizedBox(height: 16),
            Row(
              children: [
                _buildStep(1, 'Preparação'),
                _buildProgressLine(step >= 2),
                _buildStep(2, 'Transporte'),
                _buildProgressLine(step >= 3),
                _buildStep(3, 'Entrega'),
              ],
            ),
            const SizedBox(height: 16),
            Center(
              child: Text(
                step == 1
                    ? 'Seu pedido está sendo preparado.'
                    : step == 2
                    ? 'Seu pedido está a caminho!'
                    : 'Seu pedido foi entregue!',
                style: TextStyle(
                  color: Colors.green.shade700,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

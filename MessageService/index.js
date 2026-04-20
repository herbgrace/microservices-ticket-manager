const amqplib = require('amqplib');

(async () => {
    const queue = 'orders';
    const conn = await amqplib.connect('amqp://admin:dev123@localhost');

    const channel = await conn.createChannel();
    await channel.assertQueue(queue, {
        durable: true,
        arguments: {
            exclusive: false, 
            autoDelete: false
        }
    });

    // Listener
    channel.consume(queue, (msg) => {
        if (msg !== null) {
            console.log('Received:', msg.content.toJSON());
            channel.ack(msg);
        } else {
            console.log('Consumer cancelled by server');
        }
    });
})();
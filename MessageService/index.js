require('dotenv').config();
const amqplib = require('amqplib');
const nodemailer = require('nodemailer');

const transporter = nodemailer.createTransport({
    host: "smtp.ethereal.email",
    port: 587,
    secure: false,
    auth: {
        user: process.env.ETHEREAL_USERNAME,
        pass: process.env.ETHEREAL_PASSWORD,
    },
});

(async () => {
    const queue = 'orders';
    const rabbitmqUri = process.env.RABBITMQ_URI || 'amqp://admin:dev123@localhost:5672';
    
    try {
        console.log('Attempting to connect to RabbitMQ at:', rabbitmqUri);
        const conn = await amqplib.connect(rabbitmqUri);
        console.log('Connected to RabbitMQ successfully');

        const channel = await conn.createChannel();
        await channel.assertQueue(queue, {
            durable: true,
            arguments: {
                exclusive: false, 
                autoDelete: false
            }
        });

        channel.consume(queue, async (msg) => {
            if (msg !== null) {
                const info = JSON.parse(msg.content.toString());
                // console.log(info);
                const response = await transporter.sendMail({
                    from: process.env.ETHEREAL_USERNAME,
                    to: info.Email,
                    subject: "Order Successfully Placed",
                    text: info.Message
                    // TODO - go through all the tickets in order & show in email?
                    });
                
                channel.ack(msg);
                console.log('Email sent successfully:', response);
            } else {
                console.log('Consumer cancelled by server');
            }
        });

        conn.on('error', (err) => {
            console.error('RabbitMQ connection error:', err.message);
        });

        conn.on('close', () => {
            console.log('RabbitMQ connection closed, attempting to reconnect in 5s...');
            setTimeout(() => process.exit(1), 5000);
        });
    } catch (error) {
        console.error('Failed to connect to RabbitMQ:', error.message);
        console.error('Make sure RabbitMQ is running and the RABBITMQ_URI is correct');
        process.exit(1);
    }
})();
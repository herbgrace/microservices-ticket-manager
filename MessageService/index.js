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
    channel.consume(queue, async (msg) => {
        if (msg !== null) {
            const info = JSON.parse(msg.content.toString());
            console.log(info);
            const response = await transporter.sendMail({
                from: process.env.ETHEREAL_USERNAME,
                to: info.Email,
                subject: "Order Successfully Placed",
                text: info.Message
                // TODO - go through all the tickets in order & show in email?
                });
            
            channel.ack(msg);
        } else {
            console.log('Consumer cancelled by server');
        }
    });
})();
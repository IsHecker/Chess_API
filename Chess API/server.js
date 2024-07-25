const dotenv = require('dotenv');
dotenv.config({ path: './config.env' });
const mongoose = require('mongoose');


process.on('uncaughtException', (err) => {
  console.log(err.name, err.message);
  console.log('uncaught Exception: Closing Server...');
  process.exit(1);
})

const app = require('./app');

let server;

mongoose.connect(process.env.DB_URI)
  .then(conn => {
    console.log('DB is connected Successfuly!');
    server = app.listen('3000', () => {
      console.log('listening to port 3000');
    });
  })
  .catch(error => {
    console.log(error);
  });

// server = app.listen('3000', () => {
//   console.log('listening to port 3000');
// });

process.on('unhandledRejection', (err) => {
  console.log(err.name, err.message);
  console.log('unhandled Rejection: Closing Server...');
  server.close(() => {
    process.exit(1);
  })
})
const mongoose = require('mongoose');
const validator = require('validator');
const bcrypt = require('bcryptjs');
const date = require('date-and-time');

const getFullDate = () => {
  return date.format(new Date(), 'DD-MM-YYYY HH:mm:ss');
}

const getDate = (currentDate) => {
  return currentDate.split(' ')[0];
}

const puzzleSchema = new mongoose.Schema({
  name: {
    type: String,
    required: true,
    unique: true
  },
  difficulty: {
    type: String,
    enum: ['Beginner', 'Intermediate', 'Expert'],
    default: 'Beginner'
  },
  createdAt: {
    type: String,
    default: getFullDate
  },
  createdBy: {
    type: String,
    required: true
  },
  solvedBy: {
    type: Number,
    default: 0
  },
  color: {
    type: String,
    enum: ['White', 'Black'],
    default: 'White'
  },
  piecesPosition: {
    type: [String],
    required: true
  },
  solution: {
    type: [String],
    required: true
  }
});

// puzzleSchema.pre('findOneAndUpdate', function (next) {
//   this._update.createdAt = getFullDate();
//   next();
// });

puzzleSchema.pre(/^find/, function (next) {
  this.select('-__v');
  next();
});

puzzleSchema.post('find', function (doc, next) {
  doc.forEach(obj => {
    obj.createdAt = getDate(obj.createdAt);
  })
  next();
});



module.exports = mongoose.model('Puzzle', puzzleSchema);
module.exports.getFullDate = getFullDate;
const mongoose = require('mongoose');
const validator = require('validator');
const bcrypt = require('bcryptjs');

const userSchema = new mongoose.Schema({
  username: {
    type: String,
    required: true,
    unique: true
  },
  email: {
    type: String,
    validate: [validator.isEmail, 'Email is not valid!'],
    required: true,
    unique: true
  },
  password: {
    type: String,
    required: true,
    select: false
  },
  role: {
    type: String,
    enum: ['Creator', 'Player'],
    required: true
  },
  puzzles: {
    type: [String],
  },
  active: {
    type: Boolean,
    default: true,
    select: false
  }
});

userSchema.methods.comparePasswords = async function(password){
  return await bcrypt.compare(password, this.password);
}

userSchema.pre(/^find/, function(next){
  this.select('-__v');
  next();
});

module.exports = mongoose.model('User', userSchema);
const express = require('express');
const userController = require('../Controllers/userController');
const authController = require('../Controllers/authController');

const router = express.Router();


router.all('*', authController.authenticateToken);
router.route('/getuser').get(userController.getUser);
router.route('/addpuzzle').patch(userController.addPuzzle);

module.exports = router;
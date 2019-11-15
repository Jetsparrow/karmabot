-- Example JetKarmaBot database
-- (taken from mysqldump)

DROP TABLE IF EXISTS `awardtype`;
CREATE TABLE `awardtype` (
  `awardtypeid` tinyint(3) NOT NULL PRIMARY KEY,
  `commandname` varchar(35) NOT NULL UNIQUE,
  `name` varchar(32) NOT NULL,
  `symbol` varchar(16)  NOT NULL,
  `description` text NOT NULL
);

LOCK TABLES `awardtype` WRITE;
INSERT INTO `awardtype` VALUES (1,'example','Example','Examples','An example');
UNLOCK TABLES;

DROP TABLE IF EXISTS `chat`;
CREATE TABLE `chat` (
  `chatid` bigint(20) NOT NULL PRIMARY KEY,
  `locale` varchar(10) NOT NULL DEFAULT 'ru-RU',
  `isadministrator` tinyint(1) NOT NULL DEFAULT 0
);

DROP TABLE IF EXISTS `user`;
CREATE TABLE `user` (
  `userid` bigint(20) NOT NULL,
  `username` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`userid`)
);

DROP TABLE IF EXISTS `award`;
CREATE TABLE `award` (
  `awardid` int(11) NOT NULL PRIMARY KEY AUTO_INCREMENT,
  `chatid` bigint(20) NOT NULL REFERENCES `chat` (`chatid`),
  `fromid` bigint(20) NOT NULL  REFERENCES `user` (`userid`),
  `toid` bigint(20) NOT NULL REFERENCES `user` (`userid`),
  `awardtypeid` tinyint(3) NOT NULL REFERENCES `awardtype` (`awardtypeid`),
  `amount` tinyint(3) NOT NULL DEFAULT 1,
  `date` datetime NOT NULL DEFAULT current_timestamp()
);
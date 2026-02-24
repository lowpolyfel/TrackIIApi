-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: trackii
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `area`
--

DROP TABLE IF EXISTS `area`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `area` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_area_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `devices`
--

DROP TABLE IF EXISTS `devices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `devices` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `device_uid` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `location_id` int unsigned NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  `user_id` int unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `device_uid` (`device_uid`),
  UNIQUE KEY `uq_device_uid` (`device_uid`),
  KEY `fk_device_location` (`location_id`),
  KEY `idx_devices_location` (`location_id`),
  KEY `fk_device_user` (`user_id`),
  CONSTRAINT `fk_device_location` FOREIGN KEY (`location_id`) REFERENCES `location` (`id`),
  CONSTRAINT `fk_device_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `family`
--

DROP TABLE IF EXISTS `family`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `family` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_area` int unsigned NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `fk_family_area` (`id_area`),
  CONSTRAINT `fk_family_area` FOREIGN KEY (`id_area`) REFERENCES `area` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `location`
--

DROP TABLE IF EXISTS `location`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `location` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_location_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `product`
--

DROP TABLE IF EXISTS `product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `product` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_subfamily` int unsigned NOT NULL,
  `part_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_product_part` (`part_number`),
  KEY `fk_product_subfamily` (`id_subfamily`),
  KEY `idx_product_part_active` (`part_number`,`active`),
  CONSTRAINT `fk_product_subfamily` FOREIGN KEY (`id_subfamily`) REFERENCES `subfamily` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1620 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`),
  KEY `idx_role_active` (`active`),
  KEY `idx_role_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `route`
--

DROP TABLE IF EXISTS `route`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `route` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `subfamily_id` int unsigned NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `version` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `fk_route_subfamily` (`subfamily_id`),
  CONSTRAINT `fk_route_subfamily` FOREIGN KEY (`subfamily_id`) REFERENCES `subfamily` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `route_step`
--

DROP TABLE IF EXISTS `route_step`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `route_step` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `route_id` int unsigned NOT NULL,
  `step_number` int unsigned NOT NULL,
  `location_id` int unsigned NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_route_step` (`route_id`,`step_number`),
  KEY `fk_route_step_location` (`location_id`),
  CONSTRAINT `fk_route_step_location` FOREIGN KEY (`location_id`) REFERENCES `location` (`id`),
  CONSTRAINT `fk_route_step_route` FOREIGN KEY (`route_id`) REFERENCES `route` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=382 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `scan_event`
--

DROP TABLE IF EXISTS `scan_event`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `scan_event` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `wip_item_id` int unsigned NOT NULL,
  `route_step_id` int unsigned NOT NULL,
  `scan_type` enum('ENTRY','EXIT','ERROR','MANUAL') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `ts` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_scan_wip` (`wip_item_id`),
  KEY `fk_scan_step` (`route_step_id`),
  KEY `idx_scan_ts` (`ts`),
  KEY `idx_scan_event_wip_ts` (`wip_item_id`,`ts`),
  KEY `idx_scan_event_wip_step_type` (`wip_item_id`,`route_step_id`,`scan_type`),
  CONSTRAINT `fk_scan_step` FOREIGN KEY (`route_step_id`) REFERENCES `route_step` (`id`),
  CONSTRAINT `fk_scan_wip` FOREIGN KEY (`wip_item_id`) REFERENCES `wip_item` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `subfamily`
--

DROP TABLE IF EXISTS `subfamily`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `subfamily` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_family` int unsigned NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  `active_route_id` int unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_subfamily_family` (`id_family`),
  KEY `fk_subfamily_active_route` (`active_route_id`),
  CONSTRAINT `fk_subfamily_active_route` FOREIGN KEY (`active_route_id`) REFERENCES `route` (`id`),
  CONSTRAINT `fk_subfamily_family` FOREIGN KEY (`id_family`) REFERENCES `family` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `tokens`
--

DROP TABLE IF EXISTS `tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tokens` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `unregistered_parts`
--

DROP TABLE IF EXISTS `unregistered_parts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `unregistered_parts` (
  `part_id` int NOT NULL AUTO_INCREMENT,
  `part_number` varchar(255) NOT NULL,
  `creation_datetime` datetime NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`part_id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `role_id` int unsigned NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`),
  KEY `fk_user_role` (`role_id`),
  CONSTRAINT `fk_user_role` FOREIGN KEY (`role_id`) REFERENCES `role` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `wip_item`
--

DROP TABLE IF EXISTS `wip_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `wip_item` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `wo_order_id` int unsigned NOT NULL,
  `current_step_id` int unsigned NOT NULL,
  `status` enum('ACTIVE','HOLD','FINISHED','SCRAPPED') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'ACTIVE',
  `created_at` datetime NOT NULL,
  `route_id` int unsigned NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_wip_item_wo` (`wo_order_id`),
  KEY `fk_wip_item_wo` (`wo_order_id`),
  KEY `fk_wip_item_current_step` (`current_step_id`),
  KEY `fk_wip_item_route` (`route_id`),
  KEY `idx_wip_item_status` (`status`),
  KEY `idx_wip_item_current_step` (`current_step_id`),
  KEY `idx_wip_item_route` (`route_id`),
  KEY `idx_wip_status` (`status`),
  CONSTRAINT `fk_wip_item_current_step` FOREIGN KEY (`current_step_id`) REFERENCES `route_step` (`id`),
  CONSTRAINT `fk_wip_item_route` FOREIGN KEY (`route_id`) REFERENCES `route` (`id`),
  CONSTRAINT `fk_wip_item_wo` FOREIGN KEY (`wo_order_id`) REFERENCES `work_order` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `wip_rework_log`
--

DROP TABLE IF EXISTS `wip_rework_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `wip_rework_log` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `wip_item_id` int unsigned NOT NULL,
  `location_id` int unsigned NOT NULL,
  `user_id` int unsigned NOT NULL,
  `device_id` int unsigned NOT NULL,
  `qty` int unsigned NOT NULL,
  `reason` varchar(255) DEFAULT NULL,
  `created_at` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_rework_location` (`location_id`),
  KEY `fk_rework_user` (`user_id`),
  KEY `fk_rework_device` (`device_id`),
  KEY `idx_rework_wip` (`wip_item_id`,`created_at`),
  CONSTRAINT `fk_rework_device` FOREIGN KEY (`device_id`) REFERENCES `devices` (`id`),
  CONSTRAINT `fk_rework_location` FOREIGN KEY (`location_id`) REFERENCES `location` (`id`),
  CONSTRAINT `fk_rework_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`),
  CONSTRAINT `fk_rework_wip` FOREIGN KEY (`wip_item_id`) REFERENCES `wip_item` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `wip_step_execution`
--

DROP TABLE IF EXISTS `wip_step_execution`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `wip_step_execution` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `wip_item_id` int unsigned NOT NULL,
  `route_step_id` int unsigned NOT NULL,
  `user_id` int unsigned NOT NULL,
  `device_id` int unsigned NOT NULL,
  `location_id` int unsigned NOT NULL,
  `create_at` datetime NOT NULL,
  `qty_in` int unsigned NOT NULL,
  `qty_scrap` int unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_wip_step_execution` (`wip_item_id`,`route_step_id`),
  KEY `fk_exec_wip` (`wip_item_id`),
  KEY `fk_exec_step` (`route_step_id`),
  KEY `fk_exec_user` (`user_id`),
  KEY `fk_exec_device` (`device_id`),
  KEY `fk_exec_location` (`location_id`),
  KEY `idx_exec_wip_loc` (`wip_item_id`,`location_id`),
  CONSTRAINT `fk_exec_device` FOREIGN KEY (`device_id`) REFERENCES `devices` (`id`),
  CONSTRAINT `fk_exec_location` FOREIGN KEY (`location_id`) REFERENCES `location` (`id`),
  CONSTRAINT `fk_exec_step` FOREIGN KEY (`route_step_id`) REFERENCES `route_step` (`id`),
  CONSTRAINT `fk_exec_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`),
  CONSTRAINT `fk_exec_wip` FOREIGN KEY (`wip_item_id`) REFERENCES `wip_item` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `work_order`
--

DROP TABLE IF EXISTS `work_order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `work_order` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `wo_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `product_id` int unsigned NOT NULL,
  `status` enum('OPEN','IN_PROGRESS','FINISHED','CANCELLED') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'OPEN',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_work_order` (`wo_number`),
  KEY `fk_work_order_product` (`product_id`),
  KEY `idx_work_order_wo` (`wo_number`),
  CONSTRAINT `fk_work_order_product` FOREIGN KEY (`product_id`) REFERENCES `product` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-19 12:14:27

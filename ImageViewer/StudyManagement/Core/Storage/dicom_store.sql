/*
Navicat MySQL Data Transfer

Source Server         : 127.0.0.1
Source Server Version : 50715
Source Host           : localhost:3306
Source Database       : dicom_store

Target Server Type    : MYSQL
Target Server Version : 50715
File Encoding         : 65001

Date: 2019-01-22 15:32:16
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for configuration
-- ----------------------------
DROP TABLE IF EXISTS `configuration`;
CREATE TABLE `configuration` (
  `Oid` bigint(20) NOT NULL AUTO_INCREMENT,
  `Version` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Name` varchar(100) NOT NULL,
  `Value` text,
  PRIMARY KEY (`Oid`),
  UNIQUE KEY `UN_Configuration` (`Name`),
  UNIQUE KEY `UQ_Configuration` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for configurationdocument
-- ----------------------------
DROP TABLE IF EXISTS `configurationdocument`;
CREATE TABLE `configurationdocument` (
  `Oid` bigint(20) NOT NULL AUTO_INCREMENT,
  `DocumentName` varchar(255) DEFAULT NULL,
  `DocumentVersionString` varchar(30) DEFAULT NULL,
  `User` varchar(50) DEFAULT NULL,
  `InstanceKey` varchar(100) DEFAULT NULL,
  `CreationTime` datetime DEFAULT NULL,
  `DocumentText` text,
  `Version` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Oid`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for device
-- ----------------------------
DROP TABLE IF EXISTS `device`;
CREATE TABLE `device` (
  `Oid` bigint(20) NOT NULL AUTO_INCREMENT,
  `Version` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Name` varchar(64) NOT NULL,
  `Description` varchar(64) DEFAULT NULL,
  `Location` varchar(64) DEFAULT NULL,
  `AETitle` varchar(64) NOT NULL,
  `HostName` varchar(64) NOT NULL,
  `Port` int(11) NOT NULL,
  `StreamingHeaderPort` int(11) DEFAULT NULL,
  `StreamingImagePort` int(11) DEFAULT NULL,
  `IsPriorsServer` bit(1) NOT NULL,
  `ExtensionData` text,
  PRIMARY KEY (`Oid`),
  UNIQUE KEY `UN_Device` (`Name`),
  UNIQUE KEY `UQ_Device` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for study
-- ----------------------------
DROP TABLE IF EXISTS `study`;
CREATE TABLE `study` (
  `Oid` bigint(20) NOT NULL AUTO_INCREMENT,
  `Version` timestamp NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `SpecificCharacterSet` varchar(64) DEFAULT NULL,
  `StudyId` varchar(16) DEFAULT NULL,
  `StudyTimeTicks` bigint(20) DEFAULT NULL,
  `StudyTimeRaw` varchar(16) DEFAULT NULL,
  `StudyDate` datetime DEFAULT NULL,
  `StudyDateRaw` varchar(26) DEFAULT NULL,
  `AccessionNumber` varchar(16) DEFAULT NULL,
  `StudyInstanceUid` varchar(64) NOT NULL,
  `StudyDescription` varchar(64) DEFAULT NULL,
  `ModalitiesInStudy` varchar(4000) DEFAULT NULL,
  `SopClassesInStudy` varchar(4000) DEFAULT NULL,
  `NumberOfStudyRelatedSeries` int(11) DEFAULT NULL,
  `NumberOfStudyRelatedInstances` int(11) DEFAULT NULL,
  `ProcedureCodeSequenceCodingSchemeDesignator` varchar(16) DEFAULT NULL,
  `ProcedureCodeSequenceCodeValue` varchar(16) DEFAULT NULL,
  `PatientsName` varchar(256) DEFAULT NULL,
  `ReferringPhysiciansName` varchar(256) DEFAULT NULL,
  `PatientId` varchar(64) DEFAULT NULL,
  `PatientsSex` varchar(16) DEFAULT NULL,
  `PatientsBirthDate` datetime DEFAULT NULL,
  `PatientsBirthDateRaw` varchar(16) DEFAULT NULL,
  `PatientsBirthTimeTicks` bigint(20) DEFAULT NULL,
  `PatientsBirthTimeRaw` varchar(16) DEFAULT NULL,
  `PatientSpeciesDescription` varchar(64) DEFAULT NULL,
  `PatientSpeciesCodeSequenceCodingSchemeDesignator` varchar(16) DEFAULT NULL,
  `PatientSpeciesCodeSequenceCodeValue` varchar(16) DEFAULT NULL,
  `PatientSpeciesCodeSequenceCodeMeaning` varchar(64) DEFAULT NULL,
  `PatientBreedDescription` varchar(64) DEFAULT NULL,
  `PatientBreedCodeSequenceCodingSchemeDesignator` varchar(16) DEFAULT NULL,
  `PatientBreedCodeSequenceCodeValue` varchar(16) DEFAULT NULL,
  `PatientBreedCodeSequenceCodeMeaning` varchar(64) DEFAULT NULL,
  `ResponsiblePerson` varchar(256) DEFAULT NULL,
  `ResponsiblePersonRole` varchar(16) DEFAULT NULL,
  `ResponsibleOrganization` varchar(64) DEFAULT NULL,
  `SourceAETitlesInStudy` varchar(4000) DEFAULT NULL,
  `StationNamesInStudy` varchar(4000) DEFAULT NULL,
  `InstitutionNamesInStudy` varchar(4000) DEFAULT NULL,
  `StoreTime` datetime DEFAULT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Deleted` bit(1) NOT NULL,
  `Reindex` bit(1) NOT NULL,
  PRIMARY KEY (`Oid`),
  UNIQUE KEY `UN_Study` (`StudyInstanceUid`),
  UNIQUE KEY `UQ_Study` (`StudyInstanceUid`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for workitem
-- ----------------------------
DROP TABLE IF EXISTS `workitem`;
CREATE TABLE `workitem` (
  `Oid` bigint(20) NOT NULL AUTO_INCREMENT,
  `Version` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `ScheduledTime` datetime DEFAULT NULL,
  `ExpirationTime` datetime DEFAULT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `ProcessTime` datetime DEFAULT NULL,
  `RequestedTime` datetime DEFAULT NULL,
  `Status` smallint(6) DEFAULT NULL,
  `Type` varchar(32) DEFAULT NULL,
  `Priority` smallint(6) DEFAULT NULL,
  `FailureCount` smallint(6) DEFAULT NULL,
  `StudyInstanceUid` varchar(64) DEFAULT NULL,
  `SerializedRequest` text,
  `SerializedProgress` text,
  PRIMARY KEY (`Oid`),
  UNIQUE KEY `UQ_WorkItem` (`Oid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for workitemuid
-- ----------------------------
DROP TABLE IF EXISTS `workitemuid`;
CREATE TABLE `workitemuid` (
  `Oid` bigint(20) NOT NULL AUTO_INCREMENT,
  `Version` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `WorkItemOid` bigint(20) DEFAULT NULL,
  `SeriesInstanceUid` varchar(64) DEFAULT NULL,
  `SopInstanceUid` varchar(64) DEFAULT NULL,
  `Complete` bit(1) DEFAULT NULL,
  `FailureCount` tinyint(4) DEFAULT NULL,
  `Failed` bit(1) DEFAULT NULL,
  `File` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`Oid`),
  UNIQUE KEY `UQ_WorkItemUid` (`Oid`),
  KEY `WorkItem_WorkItemUid` (`WorkItemOid`),
  CONSTRAINT `WorkItem_WorkItemUid` FOREIGN KEY (`WorkItemOid`) REFERENCES `workitem` (`Oid`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

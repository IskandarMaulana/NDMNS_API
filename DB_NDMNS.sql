USE [master]
GO
/****** Object:  Database [DB_NDMNS]    Script Date: 07/07/2025 13:45:57 ******/
CREATE DATABASE [DB_NDMNS]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DB_NDMNS', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\DB_NDMNS.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DB_NDMNS_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\DB_NDMNS_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [DB_NDMNS] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DB_NDMNS].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DB_NDMNS] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DB_NDMNS] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DB_NDMNS] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DB_NDMNS] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DB_NDMNS] SET ARITHABORT OFF 
GO
ALTER DATABASE [DB_NDMNS] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DB_NDMNS] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DB_NDMNS] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DB_NDMNS] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DB_NDMNS] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DB_NDMNS] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DB_NDMNS] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DB_NDMNS] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DB_NDMNS] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DB_NDMNS] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DB_NDMNS] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DB_NDMNS] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DB_NDMNS] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DB_NDMNS] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DB_NDMNS] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DB_NDMNS] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DB_NDMNS] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DB_NDMNS] SET RECOVERY FULL 
GO
ALTER DATABASE [DB_NDMNS] SET  MULTI_USER 
GO
ALTER DATABASE [DB_NDMNS] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DB_NDMNS] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DB_NDMNS] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DB_NDMNS] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DB_NDMNS] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DB_NDMNS] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'DB_NDMNS', N'ON'
GO
ALTER DATABASE [DB_NDMNS] SET QUERY_STORE = ON
GO
ALTER DATABASE [DB_NDMNS] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [DB_NDMNS]
GO
/****** Object:  Table [dbo].[tbl_m_helpdesk]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_m_helpdesk](
	[hlp_id] [varchar](50) NOT NULL,
	[isp_id] [varchar](50) NOT NULL,
	[hlp_name] [varchar](255) NOT NULL,
	[hlp_role] [int] NOT NULL,
	[hlp_whatsappnumber] [varchar](20) NOT NULL,
	[hlp_emailaddress] [varchar](320) NOT NULL,
	[hlp_createdby] [varchar](50) NOT NULL,
	[hlp_createddate] [datetime] NOT NULL,
	[hlp_updatedby] [varchar](50) NULL,
	[hlp_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_m_helpdesk] PRIMARY KEY CLUSTERED 
(
	[hlp_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_m_isp]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_m_isp](
	[isp_id] [varchar](50) NOT NULL,
	[isp_name] [varchar](50) NOT NULL,
	[isp_whatsappgroup] [varchar](30) NOT NULL,
	[isp_emailaddress] [varchar](320) NOT NULL,
	[isp_createdby] [varchar](50) NOT NULL,
	[isp_createddate] [datetime] NOT NULL,
	[isp_updatedby] [varchar](50) NULL,
	[isp_updateddate] [datetime] NULL,
 CONSTRAINT [PK__tbl_m_is__A54AB066EDA4EF88] PRIMARY KEY CLUSTERED 
(
	[isp_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_m_network]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_m_network](
	[net_id] [varchar](50) NOT NULL,
	[sit_id] [varchar](50) NOT NULL,
	[isp_id] [varchar](50) NOT NULL,
	[net_cid] [varchar](50) NOT NULL,
	[net_name] [varchar](20) NOT NULL,
	[net_ip] [varchar](15) NOT NULL,
	[net_latency] [decimal](10, 2) NOT NULL,
	[net_status] [int] NOT NULL,
	[net_last_update] [datetime] NOT NULL,
	[net_createdby] [varchar](50) NOT NULL,
	[net_createddate] [datetime] NOT NULL,
	[net_updatedby] [varchar](50) NULL,
	[net_updateddate] [datetime] NULL,
 CONSTRAINT [PK__tbl_m_ne__EDEFA295A4ECECDD] PRIMARY KEY CLUSTERED 
(
	[net_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_m_pic]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_m_pic](
	[pic_id] [varchar](50) NOT NULL,
	[sit_id] [varchar](50) NOT NULL,
	[pic_nrp] [varchar](20) NOT NULL,
	[pic_name] [varchar](255) NOT NULL,
	[pic_role] [int] NOT NULL,
	[pic_whatsappnumber] [varchar](20) NOT NULL,
	[pic_emailaddress] [varchar](320) NULL,
	[pic_createdby] [varchar](50) NOT NULL,
	[pic_createddate] [datetime] NOT NULL,
	[pic_updatedby] [varchar](50) NULL,
	[pic_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_m_pic] PRIMARY KEY CLUSTERED 
(
	[pic_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_m_site]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_m_site](
	[sit_id] [varchar](50) NOT NULL,
	[sit_name] [varchar](10) NOT NULL,
	[sit_whatsappgroup] [varchar](30) NOT NULL,
	[sit_location] [varchar](255) NOT NULL,
	[sit_createdby] [varchar](50) NOT NULL,
	[sit_createddate] [datetime] NOT NULL,
	[sit_updatedby] [varchar](50) NULL,
	[sit_updateddate] [datetime] NULL,
 CONSTRAINT [PK__tbl_m_si__EF176310EBEEDAC8] PRIMARY KEY CLUSTERED 
(
	[sit_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_m_user]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_m_user](
	[usr_id] [varchar](50) NOT NULL,
	[usr_name] [varchar](255) NOT NULL,
	[usr_code] [varchar](10) NOT NULL,
	[usr_nrp] [varchar](10) NOT NULL,
	[usr_password] [varchar](255) NOT NULL,
	[usr_role] [varchar](255) NOT NULL,
	[usr_email] [varchar](320) NULL,
	[usr_whatsapp] [varchar](20) NULL,
	[usr_whatsappclient] [int] NOT NULL,
	[usr_status] [int] NOT NULL,
	[usr_createdby] [varchar](50) NOT NULL,
	[usr_createddate] [datetime] NOT NULL,
	[usr_updatedby] [varchar](50) NULL,
	[usr_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_m_user] PRIMARY KEY CLUSTERED 
(
	[usr_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_r_setting]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_r_setting](
	[set_id] [varchar](36) NOT NULL,
	[set_name] [varchar](255) NOT NULL,
	[set_code] [varchar](255) NOT NULL,
	[set_value] [varchar](255) NOT NULL,
	[set_createdby] [varchar](50) NOT NULL,
	[set_createddate] [datetime] NOT NULL,
	[set_updatedby] [varchar](50) NULL,
	[set_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_r_setting] PRIMARY KEY CLUSTERED 
(
	[set_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_t_detailemailhelpdesk]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_t_detailemailhelpdesk](
	[dtl_eml_id] [varchar](50) NOT NULL,
	[eml_id] [varchar](50) NOT NULL,
	[hlp_id] [varchar](50) NOT NULL,
	[dtl_eml_type] [int] NOT NULL,
	[dtl_eml_emailaddress] [varchar](320) NOT NULL,
	[dtl_eml_createdby] [varchar](50) NOT NULL,
	[dtl_eml_hlp_createddate] [datetime] NOT NULL,
	[dtl_eml_updatedby] [varchar](50) NULL,
	[dtl_eml_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_t_detailemail] PRIMARY KEY CLUSTERED 
(
	[dtl_eml_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_t_detailemailpic]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_t_detailemailpic](
	[dtl_eml_id] [varchar](50) NOT NULL,
	[eml_id] [varchar](50) NOT NULL,
	[pic_id] [varchar](50) NOT NULL,
	[dtl_eml_type] [int] NOT NULL,
	[dtl_eml_emailaddress] [varchar](320) NOT NULL,
	[dtl_eml_createdby] [varchar](50) NOT NULL,
	[dtl_eml_hlp_createddate] [datetime] NOT NULL,
	[dtl_eml_updatedby] [varchar](50) NULL,
	[dtl_eml_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_t_detailemailpic] PRIMARY KEY CLUSTERED 
(
	[dtl_eml_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_t_downtime]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_t_downtime](
	[dtm_id] [varchar](50) NOT NULL,
	[net_id] [varchar](50) NOT NULL,
	[dtm_description] [varchar](255) NOT NULL,
	[dtm_ticketnumber] [varchar](255) NOT NULL,
	[dtm_date] [datetime] NOT NULL,
	[dtm_start] [datetime] NOT NULL,
	[dtm_end] [datetime] NULL,
	[dtm_category] [int] NOT NULL,
	[dtm_subcategory] [int] NULL,
	[dtm_action] [varchar](50) NULL,
	[dtm_status] [int] NOT NULL,
	[dtm_createdby] [varchar](50) NOT NULL,
	[dtm_createddate] [datetime] NOT NULL,
	[dtm_updatedby] [varchar](50) NULL,
	[dtm_updateddate] [datetime] NULL,
 CONSTRAINT [PK__tbl_t_do__EEB34EC18B3D627E] PRIMARY KEY CLUSTERED 
(
	[dtm_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_t_email]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_t_email](
	[eml_id] [varchar](50) NOT NULL,
	[dtm_id] [varchar](50) NOT NULL,
	[eml_subject] [varchar](100) NOT NULL,
	[eml_body] [varchar](max) NOT NULL,
	[eml_image] [text] NOT NULL,
	[eml_date] [datetime] NOT NULL,
	[eml_type] [int] NOT NULL,
	[eml_status] [int] NOT NULL,
	[eml_createdby] [varchar](50) NOT NULL,
	[eml_createddate] [datetime] NOT NULL,
	[eml_updatedby] [varchar](50) NULL,
	[eml_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_t_email] PRIMARY KEY CLUSTERED 
(
	[eml_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_t_message]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_t_message](
	[msg_id] [varchar](50) NOT NULL,
	[dtm_id] [varchar](50) NOT NULL,
	[msg_date] [datetime] NOT NULL,
	[msg_recipient] [varchar](30) NOT NULL,
	[msg_recipienttype] [int] NOT NULL,
	[msg_messageid] [varchar](30) NOT NULL,
	[msg_text] [varchar](max) NOT NULL,
	[msg_image] [text] NULL,
	[msg_type] [int] NOT NULL,
	[msg_category] [int] NOT NULL,
	[msg_level] [int] NOT NULL,
	[msg_status] [int] NOT NULL,
	[msg_createdby] [varchar](50) NOT NULL,
	[msg_createddate] [datetime] NOT NULL,
	[msg_updatedby] [varchar](50) NULL,
	[msg_updateddate] [datetime] NULL,
 CONSTRAINT [PK__tbl_t_me__9CA9728D98F84557] PRIMARY KEY CLUSTERED 
(
	[msg_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbl_t_shift]    Script Date: 07/07/2025 13:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_t_shift](
	[shf_id] [varchar](50) NOT NULL,
	[usr_id] [varchar](50) NOT NULL,
	[shf_startdate] [date] NOT NULL,
	[shf_enddate] [date] NOT NULL,
	[shf_starttime] [time](0) NOT NULL,
	[shf_endtime] [time](0) NOT NULL,
	[shf_createdby] [varchar](50) NOT NULL,
	[shf_createddate] [datetime] NOT NULL,
	[shf_updatedby] [varchar](50) NULL,
	[shf_updateddate] [datetime] NULL,
 CONSTRAINT [PK_tbl_t_shift] PRIMARY KEY CLUSTERED 
(
	[shf_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_m_user] ADD  CONSTRAINT [DF_tbl_m_user_usr_whatsappclient]  DEFAULT ((0)) FOR [usr_whatsappclient]
GO
ALTER TABLE [dbo].[tbl_m_user] ADD  CONSTRAINT [DF_tbl_m_user_usr_status]  DEFAULT ((0)) FOR [usr_status]
GO
ALTER TABLE [dbo].[tbl_t_downtime] ADD  CONSTRAINT [DF_tbl_t_downtime_dtm_subcategory]  DEFAULT ((0)) FOR [dtm_subcategory]
GO
ALTER TABLE [dbo].[tbl_t_email] ADD  CONSTRAINT [DF_tbl_t_email_eml_status]  DEFAULT ((0)) FOR [eml_status]
GO
ALTER TABLE [dbo].[tbl_m_helpdesk]  WITH CHECK ADD  CONSTRAINT [FK_tbl_m_helpdesk_tbl_m_isp] FOREIGN KEY([isp_id])
REFERENCES [dbo].[tbl_m_isp] ([isp_id])
GO
ALTER TABLE [dbo].[tbl_m_helpdesk] CHECK CONSTRAINT [FK_tbl_m_helpdesk_tbl_m_isp]
GO
ALTER TABLE [dbo].[tbl_m_network]  WITH CHECK ADD  CONSTRAINT [FK__tbl_m_net__isp_i__3C69FB99] FOREIGN KEY([isp_id])
REFERENCES [dbo].[tbl_m_isp] ([isp_id])
GO
ALTER TABLE [dbo].[tbl_m_network] CHECK CONSTRAINT [FK__tbl_m_net__isp_i__3C69FB99]
GO
ALTER TABLE [dbo].[tbl_m_network]  WITH CHECK ADD  CONSTRAINT [FK__tbl_m_net__sit_i__3B75D760] FOREIGN KEY([sit_id])
REFERENCES [dbo].[tbl_m_site] ([sit_id])
GO
ALTER TABLE [dbo].[tbl_m_network] CHECK CONSTRAINT [FK__tbl_m_net__sit_i__3B75D760]
GO
ALTER TABLE [dbo].[tbl_m_pic]  WITH CHECK ADD  CONSTRAINT [FK_tbl_m_pic_tbl_m_site] FOREIGN KEY([sit_id])
REFERENCES [dbo].[tbl_m_site] ([sit_id])
GO
ALTER TABLE [dbo].[tbl_m_pic] CHECK CONSTRAINT [FK_tbl_m_pic_tbl_m_site]
GO
ALTER TABLE [dbo].[tbl_t_detailemailhelpdesk]  WITH CHECK ADD  CONSTRAINT [FK_tbl_t_detailemailisp_tbl_m_helpdesk] FOREIGN KEY([hlp_id])
REFERENCES [dbo].[tbl_m_helpdesk] ([hlp_id])
GO
ALTER TABLE [dbo].[tbl_t_detailemailhelpdesk] CHECK CONSTRAINT [FK_tbl_t_detailemailisp_tbl_m_helpdesk]
GO
ALTER TABLE [dbo].[tbl_t_detailemailhelpdesk]  WITH CHECK ADD  CONSTRAINT [FK_tbl_t_detailemailisp_tbl_t_email] FOREIGN KEY([eml_id])
REFERENCES [dbo].[tbl_t_email] ([eml_id])
GO
ALTER TABLE [dbo].[tbl_t_detailemailhelpdesk] CHECK CONSTRAINT [FK_tbl_t_detailemailisp_tbl_t_email]
GO
ALTER TABLE [dbo].[tbl_t_detailemailpic]  WITH CHECK ADD  CONSTRAINT [FK_tbl_t_detailemailpic_tbl_m_pic] FOREIGN KEY([pic_id])
REFERENCES [dbo].[tbl_m_pic] ([pic_id])
GO
ALTER TABLE [dbo].[tbl_t_detailemailpic] CHECK CONSTRAINT [FK_tbl_t_detailemailpic_tbl_m_pic]
GO
ALTER TABLE [dbo].[tbl_t_detailemailpic]  WITH CHECK ADD  CONSTRAINT [FK_tbl_t_detailemailpic_tbl_t_email] FOREIGN KEY([eml_id])
REFERENCES [dbo].[tbl_t_email] ([eml_id])
GO
ALTER TABLE [dbo].[tbl_t_detailemailpic] CHECK CONSTRAINT [FK_tbl_t_detailemailpic_tbl_t_email]
GO
ALTER TABLE [dbo].[tbl_t_downtime]  WITH CHECK ADD  CONSTRAINT [FK__tbl_t_dow__net_i__3F466844] FOREIGN KEY([net_id])
REFERENCES [dbo].[tbl_m_network] ([net_id])
GO
ALTER TABLE [dbo].[tbl_t_downtime] CHECK CONSTRAINT [FK__tbl_t_dow__net_i__3F466844]
GO
ALTER TABLE [dbo].[tbl_t_email]  WITH CHECK ADD  CONSTRAINT [FK_tbl_t_email_tbl_t_downtime] FOREIGN KEY([dtm_id])
REFERENCES [dbo].[tbl_t_downtime] ([dtm_id])
GO
ALTER TABLE [dbo].[tbl_t_email] CHECK CONSTRAINT [FK_tbl_t_email_tbl_t_downtime]
GO
ALTER TABLE [dbo].[tbl_t_message]  WITH CHECK ADD  CONSTRAINT [FK__tbl_t_mes__dtm_i__4222D4EF] FOREIGN KEY([dtm_id])
REFERENCES [dbo].[tbl_t_downtime] ([dtm_id])
GO
ALTER TABLE [dbo].[tbl_t_message] CHECK CONSTRAINT [FK__tbl_t_mes__dtm_i__4222D4EF]
GO
ALTER TABLE [dbo].[tbl_t_shift]  WITH CHECK ADD  CONSTRAINT [FK_tbl_t_shift_tbl_m_user] FOREIGN KEY([usr_id])
REFERENCES [dbo].[tbl_m_user] ([usr_id])
GO
ALTER TABLE [dbo].[tbl_t_shift] CHECK CONSTRAINT [FK_tbl_t_shift_tbl_m_user]
GO
USE [master]
GO
ALTER DATABASE [DB_NDMNS] SET  READ_WRITE 
GO
INSERT [dbo].[tbl_m_user] ([usr_id], [usr_name], [usr_code], [usr_nrp], [usr_password], [usr_role], [usr_email], [usr_whatsapp], [usr_whatsappclient], [usr_status], [usr_createdby], [usr_createddate], [usr_updatedby], [usr_updateddate]) VALUES (N'1aff27f9-5469-4bff-b00b-bf452c854793', N'Rehan M', N'RHN', N'1234567891', N'$2a$11$u2pr8u3s.NXHjYZxKtSwUOFsMCNGM19.kEaOVhN2BpkPNSuRGmJiG', N'Network Operation Center', N'rehan@gmail.com', N'6281351523928', 0, 2, N'5cd7da46-e259-4fcc-8a8d-16ad0049936b', CAST(N'2025-07-02T14:46:16.750' AS DateTime), N'5cd7da46-e259-4fcc-8a8d-16ad0049936b', CAST(N'2025-07-02T14:47:24.037' AS DateTime))
GO
INSERT [dbo].[tbl_m_user] ([usr_id], [usr_name], [usr_code], [usr_nrp], [usr_password], [usr_role], [usr_email], [usr_whatsapp], [usr_whatsappclient], [usr_status], [usr_createdby], [usr_createddate], [usr_updatedby], [usr_updateddate]) VALUES (N'5cd7da46-e259-4fcc-8a8d-16ad0049936b', N'Anjar Mulyana', N'AMY', N'1234567890', N'$2a$11$shOcH8.7l.O//QvjbggqLuBaCEnc0a/corps8EG9uYaWWUBY6af3y', N'Team Leader', N'zainal.arifin@pamapersada.com', N'628129992005', 0, 2, N'79befcee-97a7-46a4-8655-0c6f4369ff1d', CAST(N'2025-06-18T16:15:26.567' AS DateTime), NULL, NULL)
GO
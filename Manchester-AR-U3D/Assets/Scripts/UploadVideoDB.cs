using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataBank
{
	public class UploadVideoDB : SqliteHelper
    {
        private const String Tag = "Riz: LocationDb:\t";
        private const String TABLE_NAME = "newupdatevideo";
       	private const String KEY_ID = "id";
       	private const String KEY_FNAME = "filename";
       	private const String KEY_FSIZE = "filesize";
        private const String KEY_USER = "userid";
        private const String KEY_UPLOAD = "uploaded";
        private const String KEY_DATE = "date";
        private const String KEY_ORIENT = "orient";
        private const String KEY_WIDTH = "scrwidth";
        private const String KEY_HEIGHT = "scrheight";
        private String[] COLUMNS = new String[] {KEY_ID, KEY_FNAME, KEY_FSIZE, KEY_USER, KEY_UPLOAD, KEY_DATE, KEY_ORIENT, KEY_WIDTH, KEY_HEIGHT };

        public UploadVideoDB() : base()
        {
            IDbCommand dbcmd = getDbCommand();
            dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS " + TABLE_NAME + " ( " +
                KEY_ID + " INTEGER PRIMARY KEY, " +
                KEY_FNAME + " TEXT, " +
                KEY_FSIZE + " TEXT, " +
                KEY_USER + " TEXT, " +
                KEY_UPLOAD + " TEXT, " +
                KEY_DATE + " DATETIME DEFAULT CURRENT_TIMESTAMP, " +
                KEY_ORIENT + " TEXT, " +
                KEY_WIDTH + " TEXT, " +
                KEY_HEIGHT + " TEXT )";
            dbcmd.ExecuteNonQuery();

            alterTable(TABLE_NAME, KEY_ORIENT);
            alterTable(TABLE_NAME, KEY_WIDTH);
            alterTable(TABLE_NAME, KEY_HEIGHT);
        }

        public void addData(UploadVideoEntity videofile)
        {
            IDbCommand dbcmd = getDbCommand();
            dbcmd.CommandText =
                "INSERT INTO " + TABLE_NAME
                + " ( "
                + KEY_FNAME + ", "
                + KEY_FSIZE + ", "
                + KEY_USER + ", "
                + KEY_UPLOAD +  ", "
                + KEY_ORIENT + ", "
                + KEY_WIDTH + ", "
                + KEY_HEIGHT + " ) "
                + "VALUES ( '"
                + videofile._filename + "', '"
                + videofile._filesize + "', '"
                + videofile._userid + "', '"
                + videofile._uploaded + "', '"
                + videofile._orient + "', '"
                + videofile._scrwidth + "', '"
                + videofile._scrheight + "' )";
            dbcmd.ExecuteNonQuery();
        }

        public override IDataReader getDataById(int id)
        {
            return base.getDataById(id);
        }

        public override IDataReader getDataByString(string str)
        {
            Debug.Log(Tag + "Getting video: " + str);

            IDbCommand dbcmd = getDbCommand();
            dbcmd.CommandText =
                "SELECT * FROM " + TABLE_NAME + " WHERE " + KEY_ID + " = " + str + "";
            return dbcmd.ExecuteReader();
        }

        public override void deleteDataByString(string id)
        {
            Debug.Log(Tag + "Deleting video: " + id);

            IDbCommand dbcmd = getDbCommand();
            dbcmd.CommandText = "DELETE FROM " + TABLE_NAME + " WHERE " + KEY_ID + " = " + id + "";
            dbcmd.ExecuteNonQuery();
        }

        public override void deleteDataById(int id)
        {
            base.deleteDataById(id);
        }

        public override void deleteAllData()
        {
            Debug.Log(Tag + "Deleting Table");
            base.deleteAllData(TABLE_NAME);
        }

        public override IDataReader getAllData()
        {
            return base.getAllData(TABLE_NAME);
        }
        
        public IDataReader getFirstElement()
        {
            IDbCommand dbcmd = getDbCommand();
            dbcmd.CommandText = "SELECT * FROM " + TABLE_NAME + " ORDER BY " + KEY_ID + " ASC LIMIT 1";
            return dbcmd.ExecuteReader();
        }

        public IDataReader getLatestTimeStamp()
        {
            IDbCommand dbcmd = getDbCommand();
            dbcmd.CommandText = "SELECT * FROM " + TABLE_NAME + " ORDER BY " + KEY_DATE + " DESC LIMIT 1";
            return dbcmd.ExecuteReader();
        }
	}
}

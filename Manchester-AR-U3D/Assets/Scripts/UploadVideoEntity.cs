using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBank
{
    public class UploadVideoEntity
    {

        public int      _id;
        public String   _filename;
        public String   _filesize;
        public String   _userid;
        public String   _uploaded;
        public String   _dateCreated;
        public String   _orient;
        public String   _scrwidth;
        public String   _scrheight;

        public UploadVideoEntity(String filename, string filesize, string userid, string uploaded, string orient, string width, string height)
        {
            _filename = filename;
            _filesize = filesize;
            _uploaded = uploaded;
            _userid = userid;
            _dateCreated = "";
            _orient = orient;
            _scrwidth = width;
            _scrheight = height;
        }

        public UploadVideoEntity(int id, String filename, string filesize, string userid, string uploaded, string dateCreated, string orient, string width, string height)
        {
            _id = id;
            _filename = filename;
            _filesize = filesize;
            _userid = userid;
            _uploaded = uploaded;
            _dateCreated = dateCreated;
            _orient = orient;
            _scrwidth = width;
            _scrheight = height;
        }

        public static UploadVideoEntity getDefault()
        {
            return new UploadVideoEntity(0,"","0","0","0","","0","0","0");
        }
    }
}
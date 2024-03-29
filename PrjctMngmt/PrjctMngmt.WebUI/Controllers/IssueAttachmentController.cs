﻿/*
Copyright (c) 2011 Petri Tuononen

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PrjctMngmt.Models;
using System.IO;

namespace PrjctMngmt.Controllers
{
    [Authorize]
    public class IssueAttachmentController : Controller
    {
        private EntityModelContainer _dataModel = new EntityModelContainer();

        private static string basePath = AppDomain.CurrentDomain.BaseDirectory + "Uploads\\IssueAttachments\\";

        //
        // GET: /IssueAttachment/

        public ActionResult Index()
        {
            return View(_dataModel.IssueAttachments.OrderBy(i => i.Filename).ToList());
        }

        //
        // GET: /IssueAttachment/Details/5

        public ActionResult Details(int id)
        {
            return View(GetIssueAttachmentByID(id));
        }

        //
        // GET: /IssueAttachment/Create

        public ActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        } 

        //
        // POST: /IssueAttachment/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "IssueAttachmentID")]IssueAttachment newIssueAttachment, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
                return View();

            try
            {
                //Save file to server if user selected a file
                if (file != null && file.ContentLength > 0)
                {
                    newIssueAttachment.Filename = Path.GetFileName(file.FileName);
                    newIssueAttachment.MimeType = file.ContentType;
                    var path = Path.Combine(basePath, newIssueAttachment.Filename);
                    file.SaveAs(path);
                }

                newIssueAttachment.DeveloperID = _dataModel.Developers.Single(d => d.UserName == User.Identity.Name).DeveloperID;
                newIssueAttachment.EntryDate = DateTime.Now;
                _dataModel.AddToIssueAttachments(newIssueAttachment);
                _dataModel.SaveChanges();

                return RedirectToAction("Index", "Issue");
            }
            catch
            {
                return RedirectToAction("Index", "Issue");
            }
        }
        
        //
        // GET: /IssueAttachment/Edit/5
 
        public ActionResult Edit(int id)
        {
            PopulateDropDownLists();
            try
            {
                return View(GetIssueAttachmentByID(id));
            }
            catch
            {
                return RedirectToAction("Index", "Issue");
            }
        }

        //
        // POST: /IssueAttachment/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, HttpPostedFileBase file, FormCollection collection)
        {
            if (!ModelState.IsValid)
                return View();

            try
            {
                //Save file to server if user selected a file
                if (file != null && file.ContentLength > 0)
                {
                    //remove old file first
                    IssueAttachment issueAttcmt = GetIssueAttachmentByID(id);
                    var path = "";
                    if (issueAttcmt != null)
                    {
                        path = Path.Combine(basePath, issueAttcmt.Filename);
                        System.IO.File.Delete(path);
                    }
                    //save new file
                    issueAttcmt.Filename = Path.GetFileName(file.FileName);
                    issueAttcmt.MimeType = file.ContentType;
                    path = Path.Combine(basePath, issueAttcmt.Filename);
                    file.SaveAs(path);

                    _dataModel.SaveChanges();
                }

                return RedirectToAction("Index", "Issue");
            }
            catch
            {
                return RedirectToAction("Index", "Issue");
            }
        }

        //
        // GET: /IssueAttachment/Delete/5
 
        public ActionResult Delete(int id)
        {
            try
            {
                IssueAttachment issueAttcmt = GetIssueAttachmentByID(id);

                if (issueAttcmt == null)
                    return RedirectToAction("Index", "Issue");
                else
                    return View(issueAttcmt);
            }
            catch
            {
                return RedirectToAction("Index", "Issue");
            }
        }

        //
        // POST: /IssueAttachment/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                IssueAttachment issueAtchmnt = GetIssueAttachmentByID(id);

                if (issueAtchmnt == null)
                    return RedirectToAction("Index", "Issue");
                else
                {
                    //delete file from the server
                    FileInfo file = new FileInfo(basePath + issueAtchmnt.Filename);
                    file.Delete();

                    //delete entry from database
                    _dataModel.DeleteObject(issueAtchmnt);
                    _dataModel.SaveChanges();
                }

                return RedirectToAction("Index", "Issue");
            }
            catch
            {
                return RedirectToAction("Index", "Issue");
            }
        }

        public FilePathResult GetFile(int id)
        {
            IssueAttachment issueAtchmnt = GetIssueAttachmentByID(id);
            if (issueAtchmnt != null)
            {
                try
                {
                    string filename = issueAtchmnt.Filename;
                    return File(basePath + filename, issueAtchmnt.MimeType);
                }
                catch
                {
                    //TODO: log error - file no longer exists on server
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void PopulateDropDownLists()
        {
            ViewData["Issues"] = new SelectList(_dataModel.Issues.OrderBy(i => i.Subject), "IssueID", "Subject");
        }

        public IssueAttachment GetIssueAttachmentByID(int id)
        {
            try
            {
                return _dataModel.IssueAttachments.Single(i => i.IssueAttachmentID == id);
            }
            catch
            {
                return null;
            }
        }

        public ActionResult ShowIssueAttachments(int id)
        {
            ViewBag.IssueName = _dataModel.Issues.Single(i => i.IssueID == id).Subject;
            return View(GetIssueAttachmentsByID(id));
        }

        public List<IssueAttachment> GetIssueAttachmentsByID(int id) 
        {
            try
            {
                IEnumerable<IssueAttachment> results = _dataModel.IssueAttachments.Where(i => i.IssueID == id).OrderBy(i => i.Filename);
                return results.Cast<IssueAttachment>().ToList();
            }
            catch
            {
                return null;
            }
        }
    }
}

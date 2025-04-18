using FTPServer.dto;
using FTPServer.dto.dbdto;
using FTPServer.dto.requests;
using FTPServer.dto.responses;
using lib;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FTPServer
{
    internal class Controller
    {
        private static string privateKey = "deCsf55CnPBMNOlqLyzlb+6w2Vud1dFWYRuG+q+bjJ2zZk2MDIJc12RJtjuCPvoZMtVy+dQ5MbrQnlnnYRUZls8+JBMyC4zHXzGIUBwBuLeLJ9a1VYWKsgs9UmiMit2lhJUg788Phvx04X5JCXP//reLY2WGVeJvR0hhtl8B2zIlUOjQOX3mIKKmZ+g7HOGk";

        private static FILE_SYSTEMEntities dbContext = new FILE_SYSTEMEntities();
        // authen
        public static void LoginController(Socket clientSocket, GlobalRequest globalRequest)
        {
            LoginRequest request = ConverTo<LoginRequest>(globalRequest.RequestObject);
            User user = dbContext.Users.FirstOrDefault(item => item.Username == request.Username && item.Password == request.Password);
            string id = user == null ? String.Empty : user.UserId;

            if (String.IsNullOrEmpty(id))
            {
                TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
                {
                    AuthentToken = String.Empty,
                    Route = globalRequest.Route,
                    RequestObject = new LoginResponse() { Token = String.Empty }
                });
                return;
            }
            string token = TokenUltil.GenerateToken(id, privateKey);
            // send back to client
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                AuthentToken = token,
                Route = globalRequest.Route,
                RequestObject = new LoginResponse() { Token = token }
            });
        }

        public static void SignUpController(Socket clientSocket, GlobalRequest globalRequest)
        {
            SignupRequest request = ConverTo<SignupRequest>(globalRequest.RequestObject);
            // check if username is existed?
            if (UsernameExisted(request.Username))
            {
                TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
                {
                    AuthentToken = globalRequest.AuthentToken,
                    Route = globalRequest.Route,
                    RequestObject = new SignupResponse()
                    {
                        Status = ResponseStatus.ACCOUNT_ALREADY_EXISTS,
                        Message = ResponseStatus.ACCOUNT_ALREADY_EXISTS_MESSAGE
                    }
                });
                return;
            }

            User user = new User()
            {
                UserId = GetId(),
                Username = request.Username,
                Password = request.Password,
            };

            dbContext.Users.Add(user);

            dbContext.SaveChangesAsync();
            // send response
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                AuthentToken = globalRequest.AuthentToken,
                Route = globalRequest.Route,
                RequestObject = new SignupResponse()
                {
                    Status = ResponseStatus.SUCCESS,
                    Message = ResponseStatus.SUCCESS_MESSAGE
                }
            });
        }

        // List
        public static void ListController(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<ListResponse>(clientSocket, globalRequest)) return;
            ListRequest request = ConverTo<ListRequest>(globalRequest.RequestObject);
            string userId = globalRequest.AuthentToken.Split('.')[0];
            var compositeItems = dbContext.CompositeItems.Where(item => item.UserId == userId && item.ParentPath == request.FolderPath).ToList();
            List<CompositeItemDTO> folders = new List<CompositeItemDTO>();
            List<CompositeItemDTO> files = new List<CompositeItemDTO>();
            foreach (var item in compositeItems)
            {
                if (item.ItemType == CompositeConstance.FOLDER)
                {
                    folders.Add(new CompositeItemDTO(item));
                }
                else
                {
                    files.Add(new CompositeItemDTO(item));
                }
            }
            //Send back data
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new ListResponse()
                {
                    Files = files,
                    Folders = folders,
                }
            });
        }

        // Folder
        public static void AddFolder(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderAddResponse>(clientSocket, globalRequest)) return;
            FolderAddRequest request = ConverTo<FolderAddRequest>(globalRequest.RequestObject);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            if (!String.IsNullOrEmpty(FolderExisted($"{request.ParrentPath}/{request.FolderName}", userId)))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            }
            else
            {
                dbContext.CompositeItems.Add(new CompositeItem()
                {
                    ItemId = GetId(),
                    ItemPath = $"{request.ParrentPath}/{request.FolderName}",
                    ItemName = request.FolderName,
                    ItemType = CompositeConstance.FOLDER,
                    ParentPath = request.ParrentPath,
                    UserId = userId,
                    DateModify = DateTime.Now,
                });
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderAddResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        public static void UpdateFolderName(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderUpdateResponse>(clientSocket, globalRequest)) return;
            FolderUpdateRequest request = ConverTo<FolderUpdateRequest>(globalRequest.RequestObject);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string folderId = FolderExisted(request.FolderPath, userId);
            if (String.IsNullOrEmpty(folderId))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            }
            else
            {
                CompositeItem folder = dbContext.CompositeItems.FirstOrDefault(item => item.ItemId == folderId);
                folder.ItemName = request.FolderName;
                folder.ItemPath = $"{folder.ParentPath}/{request.FolderName}";
                folder.DateModify = DateTime.Now;
                dbContext.SaveChangesAsync();
            }
            Console.WriteLine("Folder path: " + request.FolderPath + " | FolderId: " + folderId);
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderUpdateResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        public static void MoveFolder(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderMoveResponse>(clientSocket, globalRequest)) return;
            FolderMoveRequest request = ConverTo<FolderMoveRequest>(globalRequest.RequestObject);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string folderId = FolderExisted(request.FolderPath, userId);
            if (String.IsNullOrEmpty(folderId))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            } else
            {
                CompositeItem curentFolder = dbContext.CompositeItems.FirstOrDefault(folder => folder.ItemId == folderId);

                // đổi path item con
                var childItems = dbContext.CompositeItems.Where(item => item.ParentPath == curentFolder.ItemPath);
                foreach(CompositeItem childItem in childItems)
                {
                    childItem.ParentPath = request.FolderNewPath;
                    childItem.ItemPath = $"{request.FolderNewPath}/{childItem.ItemName}";
                    childItem.DateModify = DateTime.Now;
                }
                curentFolder.ItemPath = request.FolderNewPath;
                curentFolder.ParentPath = request.FolderNewPath.Substring(0, request.FolderNewPath.Length - curentFolder.ItemName.Length - 1);
                curentFolder.DateModify = DateTime.Now;
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderMoveResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        public static void DeleteFolder(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderDeleteResponse>(clientSocket, globalRequest)) return;
            FolderDeleteRequest request = ConverTo<FolderDeleteRequest>(globalRequest.RequestObject);
            string userId = globalRequest.AuthentToken.Split('.')[0];
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            // find folder
            string folderId = FolderExisted(request.FolderPath, userId);
            
            if (String.IsNullOrEmpty(folderId))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            } else
            {
                // find all file and folder inside
                var composites = dbContext.CompositeItems.Where(item => item.ParentPath == request.FolderPath).ToList();
                // delete all inside item in server store
                DeleteItemInStore(composites);
                // delete all inside item in db
                dbContext.CompositeItems.RemoveRange(composites);
                // delete folder
                dbContext.CompositeItems.Remove(dbContext.CompositeItems.FirstOrDefault(item => item.ItemId == folderId));
                dbContext.SaveChangesAsync();
            }
            // send response
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderDeleteResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        public static void CopyFolder(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FolderCopyResponse>(clientSocket, globalRequest)) return;
            FolderCopyRequest request = ConverTo<FolderCopyRequest>(globalRequest.RequestObject);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];

            CompositeItem folder = dbContext.CompositeItems.FirstOrDefault(item => item.ItemId == request.FolderId);
            string folderName = folder.ItemName + "-Copy";
            string folderPath = request.DestinationPath + "/" + folderName;
            // thêm db
            dbContext.CompositeItems.Add(new CompositeItem()
            {
                ItemId = GetId(),
                ItemPath = folderPath,
                ParentPath = request.DestinationPath,
                ItemName = folderName,
                UserId = userId,
                ItemType = CompositeConstance.FOLDER,
                CopyFrom = request.FolderId,
                DateModify = DateTime.Now,
            });
            // chuyển tất cả con của folder gốc sang folder copy
            // lấy tất cả children từ folder gốc
            var compositeItems = dbContext.CompositeItems.Where(item => item.ParentPath == folder.ItemPath).ToList();
            var copyCompositeItems = compositeItems.Select(item => new CompositeItem()
            {
                ItemId = GetId(),
                ItemPath = folderPath + "/" + item.ItemName,
                ParentPath = folderPath,
                ItemName = item.ItemName,
                UserId = item.UserId,
                ItemType = item.ItemType,
                CopyFrom = item.ItemId,
                DateModify = DateTime.Now,
            }).ToList();
            dbContext.CompositeItems.AddRange(copyCompositeItems);

            dbContext.SaveChangesAsync();

            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FolderCopyResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        // File
        public static void AddFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileAddResponse>(clientSocket, globalRequest)) return;
            FileAddRequest request = ConverTo<FileAddRequest>(globalRequest.RequestObject);
            string userId = globalRequest.AuthentToken.Split('.')[0];
            // check if file existed
            string fileId = FileExisted($"{request.FolderPath}/{request.FileName}", userId);
            if (String.IsNullOrEmpty(fileId))
            {
                // generate file id id not exist.
                fileId = GetId();
            }
            Socket clientFileSocket = Server.GetFileTranferClientSocket(request.IpEndPoint);
            // send response ready to upload.
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                AuthentToken = globalRequest.AuthentToken,
                Route = globalRequest.Route,
                RequestObject = new FileAddResponse()
                {
                    Status = ResponseStatus.READY,
                    Message = ResponseStatus.READY_MESSAGE
                }
            });
            // save file to server store
            HandleUploadFile(clientFileSocket, fileId, request.Size);
            // save file to server db
            dbContext.CompositeItems.AddOrUpdate(new CompositeItem()
            {
                ItemId = fileId,
                ItemPath = $"{request.FolderPath}/{request.FileName}",
                ParentPath = request.FolderPath,
                ItemName = request.FileName,
                UserId = userId,
                ItemType = CompositeConstance.FILE,
                DateModify = DateTime.Now,
            });
            dbContext.SaveChanges();
        }

        public static void DownloadFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileDowloadResponse>(clientSocket, globalRequest)) return;
            FileDownloadRequest request = ConverTo<FileDownloadRequest>(globalRequest.RequestObject);
            string userId = globalRequest.AuthentToken.Split('.')[0];
            // nhận thông tin file
            CompositeItem file = dbContext.CompositeItems.FirstOrDefault(item => item.ItemPath == request.FilePath && item.UserId == userId);
            // lấy socket client
            Socket clientFileSocket = Server.GetFileTranferClientSocket(request.ClientEndpoint);
            string filePath = File.Exists(StandardizeFilePath(file.ItemId)) ? StandardizeFilePath(file.ItemId) : StandardizeFilePath(file.CopyFrom);
            FileInfo fileInfo = new FileInfo(filePath);
            
            // gửi response sẳn sàng kết nối
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileDowloadResponse()
                {
                    Status = ResponseStatus.READY,
                    Message = ResponseStatus.READY_MESSAGE,
                    FileSize = fileInfo.Length
                }
            });
            // bắt đầu truyền file
            HandleDownloadFile(clientFileSocket, StandardizeFilePath(filePath));
        }

        public static void UpdateFileName(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileUpdateResponse>(clientSocket, globalRequest)) return;
            FileUpdateRequest request = ConverTo<FileUpdateRequest>(globalRequest.RequestObject);
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string fileId = FileExisted(request.FilePath, userId);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            if (String.IsNullOrEmpty(fileId))
            {
                status = ResponseStatus.NOT_FOUND;
                message = ResponseStatus.NOT_FOUND_MESSAGE;
            } else
            {
                // update db
                CompositeItem compositeItem = dbContext.CompositeItems.FirstOrDefault(item => item.ItemId == fileId);
                compositeItem.ItemName = request.FileName;
                compositeItem.ItemPath = $"{compositeItem.ParentPath}/{request.FileName}";
                compositeItem.DateModify = DateTime.Now;
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileUpdateResponse()
                {
                    Status = status,
                    Message = message
                }
            });

        }

        public static void MoveFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileMoveResponse>(clientSocket, globalRequest)) return;
            FileMoveRequest request = ConverTo<FileMoveRequest>(globalRequest.RequestObject);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string fileId = FolderExisted(request.FilePath, userId);
            if (String.IsNullOrEmpty(fileId))
            {
                status = ResponseStatus.ERROR;
                message = ResponseStatus.ERROR_MESSAGE;
            }
            else
            {
                CompositeItem currentFile = dbContext.CompositeItems.FirstOrDefault(file => file.ItemId == fileId);
                currentFile.ItemPath = request.FileNewPath;
                currentFile.ParentPath = request.FileNewPath.Substring(0, request.FileNewPath.Length - currentFile.ItemName.Length - 1);
                currentFile.DateModify = DateTime.Now;
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileMoveResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        public static void CopyFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileCopyResponse>(clientSocket, globalRequest)) return;
            FileCopyRequest request = ConverTo<FileCopyRequest>(globalRequest.RequestObject);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            string userId = globalRequest.AuthentToken.Split('.')[0];
            CompositeItem compositeItem = dbContext.CompositeItems.FirstOrDefault(item => item.ItemId == request.FileId);
            // tạo item path
            string itemName = Path.GetFileNameWithoutExtension(compositeItem.ItemName) + "-Copy" + Path.GetExtension(compositeItem.ItemName);
            string itemPath = request.FolderPath + "/" + itemName;
            // thêm db
            dbContext.CompositeItems.Add(new CompositeItem()
            {
                ItemId = GetId(),
                ItemPath = itemPath,
                ParentPath = request.FolderPath,
                ItemName = itemName,
                UserId = userId,
                ItemType = CompositeConstance.FILE,
                CopyFrom = request.FileId,
                DateModify = DateTime.Now,
            });
            dbContext.SaveChangesAsync();

            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileCopyResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        public static void DeleteFile(Socket clientSocket, GlobalRequest globalRequest)
        {
            if (!DoValidateToken<FileDeleteResponse>(clientSocket, globalRequest)) return;
            FileDeleteRequest request = ConverTo<FileDeleteRequest>(globalRequest.RequestObject);
            string userId = globalRequest.AuthentToken.Split('.')[0];
            string fileId = FileExisted(request.FilePath, userId);
            int status = ResponseStatus.SUCCESS;
            string message = ResponseStatus.SUCCESS_MESSAGE;
            if (String.IsNullOrEmpty(fileId))
            {
                status = ResponseStatus.NOT_FOUND;
                message = ResponseStatus.NOT_FOUND_MESSAGE;
            }
            else
            {
                // update db
                dbContext.CompositeItems.Remove(dbContext.CompositeItems.FirstOrDefault(item => item.ItemId == fileId));
                dbContext.SaveChangesAsync();
            }
            TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
            {
                Route = globalRequest.Route,
                AuthentToken = globalRequest.AuthentToken,
                RequestObject = new FileDeleteResponse()
                {
                    Status = status,
                    Message = message
                }
            });
        }

        // private

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Return true if user existed</returns>
        private static bool UsernameExisted(string username)
        {
            User user = dbContext.Users.FirstOrDefault(x => x.Username == username);
            return user != null;
        }

        private static bool DoValidateToken<TResoibseDTO>(Socket clientSocket, GlobalRequest glRequest)
        {
            if (TokenUltil.VerifyToken(glRequest.AuthentToken, privateKey))
            {
                return true;
            }
            else
            {
                TcpProtocol.Send<GlobalResponse>(clientSocket, new GlobalResponse()
                {
                    Route = glRequest.Route,
                    AuthentToken = String.Empty,
                    RequestObject = default(TResoibseDTO)
                });
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="userId"></param>
        /// <returns>Return folder id if existed else null</returns>
        private static string FolderExisted(String folderPath, string userId)
        {
            var folder = dbContext.CompositeItems.FirstOrDefault(item => item.UserId == userId && item.ItemPath == folderPath && item.ItemType == CompositeConstance.FOLDER);
            return folder != null ? folder.ItemId : null;
        }

        private static string GetId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static void DeleteItemInStore(List<CompositeItem> composites)
        {
            // get realitic filePath
            var realiticFilePaths = new List<string>();
            foreach (var compositeItem in composites)
            {
                if(compositeItem.ItemType == CompositeConstance.FILE)
                {
                    realiticFilePaths.Add(StandardizeFilePath(compositeItem.ItemId));
                }
            }
            foreach(var path in realiticFilePaths)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="userId"></param>
        /// <returns>Return file id if existed else null</returns>
        private static string FileExisted(string filePath, string userId)
        {
            var file = dbContext.CompositeItems.FirstOrDefault(item => item.UserId == userId && item.ItemPath == filePath);
            return file != null ? file.ItemId : null;
        }

        private static void HandleUploadFile(Socket clientFileSocket, string fileId, long size)
        {
            FileTranferHelper.ReceiveFileFrom(clientFileSocket, CompositeConstance.ROOT_FOLDER_PATH, fileId);
            Server.RemoveSocket(clientFileSocket.ToString());
            clientFileSocket.Close();
        }

        private static void HandleDownloadFile(Socket clientFileSocket, string filePath)
        {
            FileTranferHelper.SendFileTo(clientFileSocket, filePath);
            while (clientFileSocket.Connected) { Thread.Sleep(100); }
            Server.RemoveSocket(clientFileSocket.ToString());
        }

        private static string StandardizeFilePath(string fileId)
        {
            return CompositeConstance.ROOT_FOLDER_PATH + fileId;
        }
    
        private static T ConverTo<T>(object value)
        {
            string json = JsonSerializer.Serialize(value);
            T request = JsonSerializer.Deserialize<T>(json);
            return request;
        }
    }
}
